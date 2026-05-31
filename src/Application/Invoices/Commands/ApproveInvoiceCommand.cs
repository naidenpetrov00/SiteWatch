using MediatR;

namespace Application.Invoices.Commands;

public class ApproveInvoiceCommand : IRequest
{
    public Guid SiteId { get; init; }
    public Guid InvoiceId { get; init; }
}

public class ApproveInvoiceCommandHandler : IRequestHandler<ApproveInvoiceCommand>
{
    public Task Handle(ApproveInvoiceCommand request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
