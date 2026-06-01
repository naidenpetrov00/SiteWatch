using Application.SeedWork.Interfaces;
using MediatR;

namespace Application.Invoices.Commands;

public sealed class ProcessInvoiceCommand : IRequest
{
    public Guid SiteId { get; init; }
    public Guid InvoiceId { get; init; }
}

public sealed class ProcessInvoiceCommandHandler(
    IInvoiceProcessingService invoiceProcessingService)
    : IRequestHandler<ProcessInvoiceCommand>
{
    public async Task Handle(ProcessInvoiceCommand request, CancellationToken cancellationToken)
    {
        await invoiceProcessingService.ProcessAsync(request.SiteId, request.InvoiceId, cancellationToken);
    }
}
