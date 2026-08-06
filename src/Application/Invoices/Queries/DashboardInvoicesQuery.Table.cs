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
                    invoice => invoice.InvoiceNumber ?? string.Empty
                ),
                TableFilterDescriptor<Invoice, DashboardInvoicesQuery>.DateTimeOffsetSearch(
                    "date",
                    query => query.Date,
                    invoice => invoice.Date
                ),
                TableFilterDescriptor<Invoice, DashboardInvoicesQuery>.DateTimeOffsetSearch(
                    "created",
                    query => query.Created,
                    invoice => (DateTimeOffset?)invoice.Created
                ),
                TableFilterDescriptor<Invoice, DashboardInvoicesQuery>.TextContains(
                    "submittedFromSiteName",
                    query => query.SubmittedFromSiteName,
                    SubmittedFromSiteDisplayLabel
                ),
                TableFilterDescriptor<Invoice, DashboardInvoicesQuery>.TextContains(
                    "taxIdentifier",
                    query => query.TaxIdentifier,
                    invoice => invoice.TaxIdentifier ?? string.Empty
                ),
                TableFilterDescriptor<Invoice, DashboardInvoicesQuery>.TextContains(
                    "address",
                    query => query.Address,
                    invoice => invoice.Address ?? string.Empty
                ),
                TableFilterDescriptor<Invoice, DashboardInvoicesQuery>.TextContains(
                    "email",
                    query => query.Email,
                    invoice => invoice.Email ?? string.Empty
                ),
                TableFilterDescriptor<Invoice, DashboardInvoicesQuery>.TextContains(
                    "phoneNumber",
                    query => query.PhoneNumber,
                    invoice => invoice.PhoneNumber ?? string.Empty
                ),
                TableFilterDescriptor<Invoice, DashboardInvoicesQuery>.TextContains(
                    "contactPerson",
                    query => query.ContactPerson,
                    invoice => invoice.ContactPerson ?? string.Empty
                ),
                TableFilterDescriptor<Invoice, DashboardInvoicesQuery>.DateTimeOffsetSearch(
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
                    invoice => invoice.PaymentMethod ?? string.Empty
                ),
                TableFilterDescriptor<Invoice, DashboardInvoicesQuery>.BooleanEquals(
                    "isComplete",
                    query => query.IsComplete,
                    invoice => invoice.Status == InvoiceStatus.Complete
                )
            ],
            Sorts: new Dictionary<string, TableSortDescriptor<Invoice, DashboardInvoicesQuery>>(
                StringComparer.OrdinalIgnoreCase
            )
            {
                ["numberId"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "numberId",
                    invoice => invoice.NumberId,
                    invoice => invoice.Id
                ),
                ["id"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "id",
                    invoice => invoice.Id
                ),
                ["isComplete"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "isComplete",
                    invoice => invoice.Status == InvoiceStatus.Complete,
                    invoice => invoice.Id
                ),
                ["supplierId"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "supplierId",
                    SupplierDisplayLabel,
                    invoice => invoice.Id
                ),
                ["invoiceNumber"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "invoiceNumber",
                    invoice => invoice.InvoiceNumber ?? string.Empty,
                    invoice => invoice.Id
                ),
                ["date"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "date",
                    invoice => invoice.Date ?? DateTimeOffset.MinValue,
                    invoice => invoice.Id
                ),
                ["created"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "created",
                    invoice => invoice.Created,
                    invoice => invoice.Id
                ),
                ["submittedFromSiteName"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "submittedFromSiteName",
                    SubmittedFromSiteDisplayLabel,
                    invoice => invoice.Id
                ),
                ["taxIdentifier"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "taxIdentifier",
                    invoice => invoice.TaxIdentifier ?? string.Empty,
                    invoice => invoice.Id
                ),
                ["address"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "address",
                    invoice => invoice.Address ?? string.Empty,
                    invoice => invoice.Id
                ),
                ["email"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "email",
                    invoice => invoice.Email ?? string.Empty,
                    invoice => invoice.Id
                ),
                ["phoneNumber"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "phoneNumber",
                    invoice => invoice.PhoneNumber ?? string.Empty,
                    invoice => invoice.Id
                ),
                ["contactPerson"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "contactPerson",
                    invoice => invoice.ContactPerson ?? string.Empty,
                    invoice => invoice.Id
                ),
                ["paymentTerm"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "paymentTerm",
                    invoice => invoice.PaymentTerm ?? DateTimeOffset.MinValue,
                    invoice => invoice.Id
                ),
                ["totalValueExcludingVat"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "totalValueExcludingVat",
                    invoice => invoice.TotalValueExcludingVat ?? decimal.MinValue,
                    invoice => invoice.Id
                ),
                ["vat"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "vat",
                    invoice => invoice.Vat ?? decimal.MinValue,
                    invoice => invoice.Id
                ),
                ["totalValueIncludingVat"] = TableSortDescriptor<Invoice, DashboardInvoicesQuery>.Create(
                    "totalValueIncludingVat",
                    invoice => invoice.TotalValueIncludingVat ?? decimal.MinValue,
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
                    invoice => invoice.PaymentMethod ?? string.Empty,
                    invoice => invoice.Id
                )
            },
            DefaultSort: query =>
                query
                    .OrderByDescending(invoice => invoice.Created)
                    .ThenBy(invoice => invoice.Id)
        );

    private static readonly Expression<Func<Invoice, string>> SupplierDisplayLabel = invoice =>
        invoice.Supplier == null
            ? string.Empty
            : invoice.Supplier.Type == PersonType.Company
            ? invoice.Supplier.CompanyName ?? string.Empty
            : invoice.Supplier.MiddleName == null
                ? (invoice.Supplier.FirstName ?? string.Empty) + " " + (invoice.Supplier.LastName ?? string.Empty)
                : (invoice.Supplier.FirstName ?? string.Empty)
                    + " "
                    + (invoice.Supplier.MiddleName ?? string.Empty)
                    + " "
                    + (invoice.Supplier.LastName ?? string.Empty);

    private static readonly Expression<Func<Invoice, string>> SubmittedFromSiteDisplayLabel = invoice =>
        invoice.SubmittedFromSite == null ? string.Empty : invoice.SubmittedFromSite.Name.Value;

    private static Expression<Func<Invoice, bool>>? BuildDecimalEqualsPredicate(
        string? rawValue,
        Expression<Func<Invoice, decimal?>> selector
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

        var hasValue = Expression.Property(selector.Body, nameof(Nullable<decimal>.HasValue));
        var value = Expression.Property(selector.Body, nameof(Nullable<decimal>.Value));
        var body = Expression.AndAlso(
            hasValue,
            Expression.Equal(value, Expression.Constant(parsedValue)));
        return Expression.Lambda<Func<Invoice, bool>>(body, selector.Parameters);
    }
}
