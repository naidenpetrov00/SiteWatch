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

        if (string.IsNullOrWhiteSpace(supplier.Eik))
        {
            throw CreateValidationException(nameof(request.SupplierId), "Supplier must have an EIK.");
        }

        var address = GetRequiredAddress(supplier);
        var email = GetRequiredContactValue(supplier, ContactType.Email);
        var phoneNumber = GetRequiredContactValue(supplier, ContactType.Phone);
        var contactPerson = supplier.DisplayName;

        if (string.IsNullOrWhiteSpace(contactPerson))
        {
            throw CreateValidationException(nameof(request.SupplierId), "Supplier must have a display name.");
        }

        var date = ParseDateTimeOffset(request.Date, nameof(request.Date));
        var paymentDate = ParseOptionalDateTimeOffset(request.PaymentDate, nameof(request.PaymentDate));
        var paymentTime = ParseOptionalDateTimeOffset(request.PaymentTime, nameof(request.PaymentTime));
        var vatAmount = Math.Round(request.TotalValue * request.VatRate / 100m, 2, MidpointRounding.AwayFromZero);
        var totalValueIncludingVat = Math.Round(request.TotalValue + vatAmount, 2, MidpointRounding.AwayFromZero);

        var invoice = Invoice.Create(
            supplier.Id,
            supplier,
            request.InvoiceNumber,
            date,
            supplier.Eik,
            address,
            email,
            phoneNumber,
            contactPerson,
            request.Iban,
            request.PaymentTerm,
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
            .FirstOrDefault();

        if (address is null)
        {
            throw CreateValidationException(nameof(supplier.Addresses), "Supplier must have an address.");
        }

        var parts = new[]
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
            .ToArray();

        if (parts.Length == 0)
        {
            throw CreateValidationException(nameof(supplier.Addresses), "Supplier address must not be empty.");
        }

        return string.Join(", ", parts);
    }

    private static string GetRequiredContactValue(Person supplier, ContactType contactType)
    {
        var contact = supplier.Contacts
            .Where(item => item.IsActive && item.ContactType == contactType)
            .OrderByDescending(item => item.IsPrimary)
            .FirstOrDefault();

        if (contact is null || string.IsNullOrWhiteSpace(contact.Value))
        {
            throw CreateValidationException(
                nameof(supplier.Contacts),
                $"Supplier must have a {contactType.ToString().ToLowerInvariant()} contact."
            );
        }

        return contact.Value.Trim();
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
