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
    public async Task<List<InvoiceSummaryDto>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        var invoices = await dbContext.InvoiceDocuments
            .AsNoTracking()
            .Where(x => x.SiteId == request.SiteId)
            .OrderByDescending(x => x.Created)
            .Select(x => new InvoiceSummaryDto(
                x.Id,
                x.SiteId,
                x.FileId,
                x.FileName,
                x.ContentType,
                x.DocumentType.ToString(),
                x.ExtractionStatus.ToString(),
                x.Created))
            .ToListAsync(cancellationToken);

        return invoices;
    }
}
