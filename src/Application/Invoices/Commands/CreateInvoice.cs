using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using MediatR;

namespace Application.Invoices.Commands;

public abstract record InvoiceDetailsCommand
{
    public Guid SupplierId { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public string Date { get; init; } = string.Empty;
    public string PaymentTerm { get; init; } = string.Empty;
    public decimal TotalValue { get; init; }
    public decimal VatRate { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public string? PaymentDate { get; init; }
    public string? PaymentTime { get; init; }
    public List<InvoiceSiteAllocationInput> SiteAllocations { get; init; } = [];
}

[Authorize(Roles = UserRoles.Administrator)]
public sealed record CreateInvoiceCommand : InvoiceDetailsCommand, IRequest<Guid>;

public sealed class CreateInvoiceHandler(IInvoiceService invoiceService)
    : IRequestHandler<CreateInvoiceCommand, Guid>
{
    public Task<Guid> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken) =>
        invoiceService.CreateAsync(request, cancellationToken);
}
