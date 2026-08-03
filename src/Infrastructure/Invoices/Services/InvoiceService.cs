using System.Globalization;
using Ardalis.GuardClauses;
using Application.Invoices.Commands;
using Application.Invoices.Queries;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Application.SeedWork.Queries;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Domain.SeedWork.Enums;
using Infrastructure.Data;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Invoices.Services;

public sealed class InvoiceService(ApplicationDbContext dbContext, IMapper mapper) : IInvoiceService
{
    public async Task<Guid> CreateAsync(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var supplier = await dbContext.Persons
            .Include(person => person.Addresses)
            .Include(person => person.Contacts)
            .SingleOrDefaultAsync(person => person.Id == request.SupplierId, cancellationToken);

        if (supplier is null)
        {
            throw CreateValidationException(nameof(request.SupplierId), "Supplier must exist.");
        }

        var taxIdentifier = supplier.Type == PersonType.Company ? supplier.Eik : supplier.Egn;
        if (string.IsNullOrWhiteSpace(taxIdentifier))
        {
            var message = supplier.Type == PersonType.Company
                ? "Company is missing EIK."
                : "Individual is missing EGN.";
            throw CreateValidationException(nameof(request.SupplierId), message);
        }

        var address = GetRequiredAddress(supplier);
        var email = GetOptionalContactValue(supplier, ContactType.Email);
        var phoneNumber = GetOptionalContactValue(supplier, ContactType.Phone);
        var contactPerson = supplier.DisplayName;

        if (string.IsNullOrWhiteSpace(contactPerson))
        {
            throw CreateValidationException(nameof(request.SupplierId), "Supplier is missing a display name.");
        }

        var date = ParseDateTimeOffset(request.Date, nameof(request.Date));
        var paymentTerm = ParseDateTimeOffset(request.PaymentTerm, nameof(request.PaymentTerm));
        var paymentDate = ParseOptionalDateTimeOffset(request.PaymentDate, nameof(request.PaymentDate));
        var paymentTime = ParseOptionalDateTimeOffset(request.PaymentTime, nameof(request.PaymentTime));
        var vatAmount = Math.Round(request.TotalValue * request.VatRate / 100m, 2, MidpointRounding.AwayFromZero);
        var totalValueIncludingVat = Math.Round(request.TotalValue + vatAmount, 2, MidpointRounding.AwayFromZero);

        var invoice = Invoice.Create(
            supplier.Id,
            supplier,
            request.InvoiceNumber,
            date,
            taxIdentifier,
            address,
            email,
            phoneNumber,
            contactPerson,
            paymentTerm,
            request.TotalValue,
            vatAmount,
            totalValueIncludingVat,
            request.PaymentMethod,
            paymentDate,
            paymentTime
        );

        var sitePayments = await CreateSitePaymentsAsync(
            invoice,
            request.SiteAllocations,
            cancellationToken);
        invoice.ReplaceSitePayments(sitePayments);

        dbContext.Invoices.Add(invoice);
        await dbContext.SaveChangesAsync(cancellationToken);

        return invoice.Id;
    }

    public async Task UpdateSiteAllocationsAsync(
        UpdateInvoiceSiteAllocationsCommand request,
        CancellationToken cancellationToken)
    {
        var invoice = await dbContext.Invoices
            .Include(item => item.SitePayments)
            .SingleOrDefaultAsync(item => item.Id == request.InvoiceId, cancellationToken);

        if (invoice is null)
        {
            throw new NotFoundException(nameof(Invoice), request.InvoiceId.ToString());
        }

        if (request.SiteAllocations.Sum(allocation => allocation.Amount)
            > invoice.TotalValueIncludingVat)
        {
            throw CreateValidationException(
                nameof(request.SiteAllocations),
                "The allocated total cannot exceed the invoice total including VAT.");
        }

        var sitePayments = await CreateSitePaymentsAsync(
            invoice,
            request.SiteAllocations,
            cancellationToken);

        dbContext.SitePayments.RemoveRange(invoice.SitePayments);
        invoice.ReplaceSitePayments(sitePayments);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<DashboardInvoiceDto>> GetDashboardInvoicesAsync(
        DashboardInvoicesQuery request,
        CancellationToken cancellationToken
    )
    {
        return await dbContext.Invoices
            .AsNoTracking()
            .ToPagedResultAsync<Domain.Entities.Invoice, DashboardInvoiceDto, DashboardInvoicesQuery>(
                request,
                DashboardInvoicesQuery.Table,
                query => query.ProjectTo<DashboardInvoiceDto>(mapper.ConfigurationProvider),
                cancellationToken
            );
    }

    public async Task<IReadOnlyList<SiteInvoiceDto>> GetSiteInvoicesAsync(
        Guid siteId,
        string userId,
        CancellationToken cancellationToken)
    {
        var siteIsAccessible = await dbContext.Sites
            .AsNoTracking()
            .AnyAsync(
                site => site.Id == siteId && site.Users.Any(user => user.Id == userId),
                cancellationToken);

        if (!siteIsAccessible)
        {
            throw new NotFoundException(nameof(Site), siteId.ToString());
        }

        var invoices = await dbContext.Invoices
            .AsNoTracking()
            .Include(invoice => invoice.Supplier)
            .Include(invoice => invoice.SitePayments)
            .Where(invoice => invoice.SitePayments.Any(payment => payment.SiteId == siteId))
            .OrderByDescending(invoice => invoice.Date)
            .ThenByDescending(invoice => invoice.NumberId)
            .ToListAsync(cancellationToken);

        return invoices.Select(invoice =>
        {
            var allocation = invoice.SitePayments.Single(payment => payment.SiteId == siteId);

            return new SiteInvoiceDto
            {
                Id = invoice.Id,
                NumberId = invoice.NumberId,
                SupplierId = invoice.SupplierId,
                SupplierDisplayLabel = invoice.Supplier.DisplayName,
                InvoiceNumber = invoice.InvoiceNumber,
                Date = invoice.Date,
                TaxIdentifier = invoice.TaxIdentifier,
                Address = invoice.Address,
                Email = invoice.Email,
                PhoneNumber = invoice.PhoneNumber,
                ContactPerson = invoice.ContactPerson,
                PaymentTerm = invoice.PaymentTerm,
                TotalValueExcludingVat = invoice.TotalValueExcludingVat,
                Vat = invoice.Vat,
                TotalValueIncludingVat = invoice.TotalValueIncludingVat,
                PaymentDate = invoice.PaymentDate,
                PaymentTime = invoice.PaymentTime,
                PaymentMethod = invoice.PaymentMethod,
                SiteAllocation = new SiteInvoiceAllocationDto(
                    allocation.Amount,
                    allocation.Direction.ToString())
            };
        }).ToList();
    }

    public async Task EnsureUserCanAccessInvoiceAsync(
        Guid siteId,
        Guid invoiceId,
        string userId,
        CancellationToken cancellationToken)
    {
        var canAccess = await dbContext.SitePayments
            .AsNoTracking()
            .AnyAsync(
                payment =>
                    payment.SiteId == siteId
                    && payment.InvoiceId == invoiceId
                    && payment.Site.Users.Any(user => user.Id == userId),
                cancellationToken);

        if (!canAccess)
        {
            throw new NotFoundException(nameof(Invoice), invoiceId.ToString());
        }
    }

    public async Task EnsureInvoiceExistsAsync(
        Guid invoiceId,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.Invoices.AsNoTracking()
                .AnyAsync(invoice => invoice.Id == invoiceId, cancellationToken))
        {
            throw new NotFoundException(nameof(Invoice), invoiceId.ToString());
        }
    }

    private static string GetRequiredAddress(Person supplier)
    {
        var address = supplier.Addresses
            .Where(item => item.IsActive)
            .OrderByDescending(item => item.IsPrimary)
            .Select(address => new
            {
                Address = address,
                Parts = new[]
                {
                    address.AddressLine,
                    address.AdditionalLine,
                    address.City,
                    address.PostalCode,
                    address.Country
                }
                    .Select(value => value?.Trim())
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Select(value => value!)
                    .ToArray()
            })
            .FirstOrDefault(item => item.Parts.Length > 0);

        if (address is null)
        {
            throw CreateValidationException(
                nameof(supplier.Addresses),
                "Supplier is missing an active address."
            );
        }

        return string.Join(", ", address.Parts);
    }

    private async Task<List<SitePayment>> CreateSitePaymentsAsync(
        Invoice invoice,
        IReadOnlyCollection<InvoiceSiteAllocationInput> allocations,
        CancellationToken cancellationToken)
    {
        if (allocations.Count == 0)
        {
            return [];
        }

        var siteIds = allocations.Select(allocation => allocation.SiteId).Distinct().ToList();
        var sites = await dbContext.Sites
            .Where(site => siteIds.Contains(site.Id))
            .ToDictionaryAsync(site => site.Id, cancellationToken);

        var missingSiteId = siteIds.FirstOrDefault(siteId => !sites.ContainsKey(siteId));
        if (missingSiteId != Guid.Empty)
        {
            throw CreateValidationException(
                nameof(InvoiceSiteAllocationInput.SiteId),
                $"Site {missingSiteId} must exist.");
        }

        return allocations
            .Select(allocation => SitePayment.Create(
                invoice,
                sites[allocation.SiteId],
                allocation.Amount,
                ParseDirection(allocation.Direction)))
            .ToList();
    }

    private static SitePaymentDirection ParseDirection(string direction)
    {
        if (Enum.TryParse<SitePaymentDirection>(direction, true, out var parsedDirection)
            && Enum.IsDefined(parsedDirection))
        {
            return parsedDirection;
        }

        throw CreateValidationException(
            nameof(InvoiceSiteAllocationInput.Direction),
            "Direction must be In or Out.");
    }

    private static string GetOptionalContactValue(Person supplier, ContactType contactType)
    {
        var contact = supplier.Contacts
            .Where(item => item.IsActive && item.ContactType == contactType)
            .OrderByDescending(item => item.IsPrimary)
            .FirstOrDefault();

        return contact?.Value.Trim() ?? string.Empty;
    }

    private static DateTimeOffset ParseDateTimeOffset(string value, string parameterName)
    {
        if (!DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces,
                out var parsedValue))
        {
            throw CreateValidationException(parameterName, $"{parameterName} must be a valid date.");
        }

        return parsedValue;
    }

    private static DateTimeOffset? ParseOptionalDateTimeOffset(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces,
                out var parsedValue))
        {
            throw CreateValidationException(parameterName, $"{parameterName} must be a valid date-time.");
        }

        return parsedValue;
    }

    private static ValidationException CreateValidationException(string propertyName, string message) =>
        new(new[] { new ValidationFailure(propertyName, message) });
}
