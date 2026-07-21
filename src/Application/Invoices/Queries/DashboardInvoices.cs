using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Application.SeedWork.Queries;
using MediatR;

namespace Application.Invoices.Queries;

public sealed partial class DashboardInvoicesQuery
    : TableQueryRequest,
        IRequest<PagedResult<DashboardInvoiceDto>>
{
    public string? Id { get; set; }
    public string? SupplierId { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? Date { get; set; }
    public string? TaxIdentifier { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ContactPerson { get; set; }
    public string? Iban { get; set; }
    public string? PaymentTerm { get; set; }
    public string? TotalValueExcludingVat { get; set; }
    public string? Vat { get; set; }
    public string? TotalValueIncludingVat { get; set; }
    public string? PaymentDate { get; set; }
    public string? PaymentTime { get; set; }
    public string? PaymentMethod { get; set; }
}

public sealed class DashboardInvoicesQueryHandler(IInvoiceService invoiceService)
    : IRequestHandler<DashboardInvoicesQuery, PagedResult<DashboardInvoiceDto>>
{
    public Task<PagedResult<DashboardInvoiceDto>> Handle(
        DashboardInvoicesQuery request,
        CancellationToken cancellationToken
    ) => invoiceService.GetDashboardInvoicesAsync(request, cancellationToken);
}
