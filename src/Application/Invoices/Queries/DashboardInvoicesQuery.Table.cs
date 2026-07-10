using System.Globalization;
using System.Linq.Expressions;
using Application.SeedWork.Queries;
using Domain.Entities;
using Domain.SeedWork.Enums;

namespace Application.Invoices.Queries;

public sealed partial class DashboardInvoicesQuery
{
    public static readonly TableQueryDefinition<Invoice, DashboardInvoicesQuery> Table =
        new(
            Filters:
            [
                TableFilterDescriptorExtensions.GuidEquals<Invoice, DashboardInvoicesQuery>(
                    "id",
                    query => query.Id,
                    invoice => invoice.Id
                ),
                TableFilterDescriptor<Invoice, DashboardInvoicesQuery>.TextContains(
                    "supplierId",
                    query => query.SupplierId,
                    SupplierDisplayLabel
                ),
                TableFilterDescriptor<Invoice, DashboardInvoicesQuery>.TextContains(
                    "invoiceNumber",
                    query => query.InvoiceNumber,
                    invoice => invoice.InvoiceNumber
                ),
                TableFilterDescriptor<Invoice, DashboardInvoicesQuery>.DateTimeOffsetSearch(
                    "date",
                    query => query.Date,
                    invoice => invoice.Date
                ),
                TableFilterDescriptor<Invoice, DashboardInvoicesQuery>.TextContains(
                    "eik",
                    query => query.Eik,
                    invoice => invoice.Eik
                ),
                TableFilterDescriptor<Invoice, DashboardInvoicesQuery>.TextContains(
                    "address",
                    query => query.Address,
                    invoice => invoice.Address
                ),
                TableFilterDescriptor<Invoice, DashboardInvoicesQuery>.TextContains(
                    "email",
                    query => query.Email,
                    invoice => invoice.Email
                ),
                TableFilterDescriptor<Invoice, DashboardInvoicesQuery>.TextContains(
                    "phoneNumber",
                    query => query.PhoneNumber,
                    invoice => invoice.PhoneNumber
                ),
                TableFilterDescriptor<Invoice, DashboardInvoicesQuery>.TextContains(
                    "contactPerson",
                    query => query.ContactPerson,
                    invoice => invoice.ContactPerson
                ),
                TableFilterDescriptor<Invoice, DashboardInvoicesQuery>.TextContains(
                    "iban",
                    query => query.Iban,
                    invoice => invoice.Iban
                ),
                TableFilterDescriptor<Invoice, DashboardInvoicesQuery>.TextContains(
                    "paymentTerm",
                    query => query.PaymentTerm,
                    invoice => invoice.PaymentTerm
                ),
                new TableFilterDescriptor<Invoice, DashboardInvoicesQuery>(
                    "totalValueExcludingVat",
                    request => BuildDecimalEqualsPredicate(
                        request.TotalValueExcludingVat,
                        invoice => invoice.TotalValueExcludingVat
                    )
                ),
                new TableFilterDescriptor<Invoice, DashboardInvoicesQuery>(
                    "vat",
                    request => BuildDecimalEqualsPredicate(request.Vat, invoice => invoice.Vat)
                ),
                new TableFilterDescriptor<Invoice, DashboardInvoicesQuery>(
                    "totalValueIncludingVat",
                    request => BuildDecimalEqualsPredicate(
                        request.TotalValueIncludingVat,
                        invoice => invoice.TotalValueIncludingVat
                    )
                ),
                TableFilterDescriptor<Invoice, DashboardInvoicesQuery>.DateTimeOffsetSearch(
                    "paymentDate",
                    query => query.PaymentDate,
                    invoice => invoice.PaymentDate
                ),
                TableFilterDescriptor<Invoice, DashboardInvoicesQuery>.DateTimeOffsetSearch(
                    "paymentTime",
                    query => query.PaymentTime,
                    invoice => invoice.PaymentTime
                ),
                TableFilterDescriptor<Invoice, DashboardInvoicesQuery>.TextContains(
                    "paymentMethod",
                    query => query.PaymentMethod,
                    invoice => invoice.PaymentMethod
                )
            ],
            Sorts: new Dictionary<string, TableSortDescriptor<Invoice, DashboardInvoicesQuery>>(
                StringComparer.OrdinalIgnoreCase
            )
            {
                ["id"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "id",
                    invoice => invoice.Id
                ),
                ["supplierId"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "supplierId",
                    SupplierDisplayLabel,
                    invoice => invoice.Id
                ),
                ["invoiceNumber"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "invoiceNumber",
                    invoice => invoice.InvoiceNumber,
                    invoice => invoice.Id
                ),
                ["date"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "date",
                    invoice => invoice.Date,
                    invoice => invoice.Id
                ),
                ["eik"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "eik",
                    invoice => invoice.Eik,
                    invoice => invoice.Id
                ),
                ["address"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "address",
                    invoice => invoice.Address,
                    invoice => invoice.Id
                ),
                ["email"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "email",
                    invoice => invoice.Email,
                    invoice => invoice.Id
                ),
                ["phoneNumber"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "phoneNumber",
                    invoice => invoice.PhoneNumber,
                    invoice => invoice.Id
                ),
                ["contactPerson"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "contactPerson",
                    invoice => invoice.ContactPerson,
                    invoice => invoice.Id
                ),
                ["iban"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "iban",
                    invoice => invoice.Iban,
                    invoice => invoice.Id
                ),
                ["paymentTerm"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "paymentTerm",
                    invoice => invoice.PaymentTerm,
                    invoice => invoice.Id
                ),
                ["totalValueExcludingVat"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "totalValueExcludingVat",
                    invoice => invoice.TotalValueExcludingVat,
                    invoice => invoice.Id
                ),
                ["vat"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "vat",
                    invoice => invoice.Vat,
                    invoice => invoice.Id
                ),
                ["totalValueIncludingVat"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "totalValueIncludingVat",
                    invoice => invoice.TotalValueIncludingVat,
                    invoice => invoice.Id
                ),
                ["paymentDate"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "paymentDate",
                    invoice => invoice.PaymentDate ?? DateTimeOffset.MinValue,
                    invoice => invoice.Id
                ),
                ["paymentTime"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "paymentTime",
                    invoice => invoice.PaymentTime ?? DateTimeOffset.MinValue,
                    invoice => invoice.Id
                ),
                ["paymentMethod"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "paymentMethod",
                    invoice => invoice.PaymentMethod,
                    invoice => invoice.Id
                )
            },
            DefaultSort: query =>
                query
                    .OrderByDescending(invoice => invoice.Date)
                    .ThenBy(invoice => invoice.Id)
        );

    private static readonly Expression<Func<Invoice, string>> SupplierDisplayLabel = invoice =>
        invoice.Supplier.Type == PersonType.Company
            ? invoice.Supplier.CompanyName ?? string.Empty
            : invoice.Supplier.MiddleName == null
                ? (invoice.Supplier.FirstName ?? string.Empty) + " " + (invoice.Supplier.LastName ?? string.Empty)
                : (invoice.Supplier.FirstName ?? string.Empty)
                    + " "
                    + (invoice.Supplier.MiddleName ?? string.Empty)
                    + " "
                    + (invoice.Supplier.LastName ?? string.Empty);

    private static Expression<Func<Invoice, bool>>? BuildDecimalEqualsPredicate(
        string? rawValue,
        Expression<Func<Invoice, decimal>> selector
    )
    {
        var normalizedValue = rawValue?.Trim() ?? string.Empty;
        if (normalizedValue.Length == 0)
        {
            return null;
        }

        if (
            !decimal.TryParse(
                normalizedValue,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var parsedValue
            )
        )
        {
            return null;
        }

        var body = Expression.Equal(selector.Body, Expression.Constant(parsedValue));
        return Expression.Lambda<Func<Invoice, bool>>(body, selector.Parameters);
    }
}
