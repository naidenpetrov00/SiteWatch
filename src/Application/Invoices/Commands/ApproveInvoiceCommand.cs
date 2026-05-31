using Application.SeedWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace Application.Invoices.Commands;

public class ApproveInvoiceCommand : IRequest
{
    public Guid SiteId { get; init; }
    public Guid InvoiceId { get; init; }
}

public class ApproveInvoiceCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<ApproveInvoiceCommand>
{
    public async Task Handle(ApproveInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await dbContext.InvoiceDocuments.FirstOrDefaultAsync(
            x => x.SiteId == request.SiteId && x.Id == request.InvoiceId,
            cancellationToken);

        if (invoice is null)
        {
            return;
        }

        invoice.Approve(DateTimeOffset.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
