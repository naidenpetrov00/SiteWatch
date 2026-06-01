using Application.SeedWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Invoices.Queries;

public class GetInvoicesQuery : IRequest<List<InvoiceSummaryDto>>
{
    public Guid SiteId { get; init; }
}

public class GetInvoicesQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetInvoicesQuery, List<InvoiceSummaryDto>>
{
    public async Task<List<InvoiceSummaryDto>> Handle(
        GetInvoicesQuery request,
        CancellationToken cancellationToken)
    {
        var invoices = await dbContext.InvoiceDocuments
            .AsNoTracking()
            .Where(x => x.SiteId == request.SiteId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new InvoiceSummaryDto(
                x.Id,
                x.OriginalFileName,
                x.Status.ToString(),
                x.DocumentType.ToString(),
                x.SupplierName,
                x.InvoiceNumber,
                x.InvoiceDate,
                x.GrossTotal,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return invoices;
    }
}
