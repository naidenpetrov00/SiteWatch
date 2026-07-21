using System.Globalization;
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
            request.Iban,
            paymentTerm,
            request.TotalValue,
            vatAmount,
            totalValueIncludingVat,
            request.PaymentMethod,
            paymentDate,
            paymentTime
        );

        dbContext.Invoices.Add(invoice);
        await dbContext.SaveChangesAsync(cancellationToken);

        return invoice.Id;
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
