using MediatR;
using Microsoft.EntityFrameworkCore;
using Application.SeedWork.Interfaces;

namespace Application.Invoices.Queries;

public class GetInvoiceByIdQuery : IRequest<InvoiceDetailsDto?>
{
    public Guid SiteId { get; init; }
    public Guid InvoiceId { get; init; }
}

public class GetInvoiceByIdQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetInvoiceByIdQuery, InvoiceDetailsDto?>
{
    public async Task<InvoiceDetailsDto?> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await dbContext.InvoiceDocuments
            .AsNoTracking()
            .Include(x => x.Lines)
            .Include(x => x.ReviewIssues)
            .FirstOrDefaultAsync(
                x => x.SiteId == request.SiteId && x.Id == request.InvoiceId,
                cancellationToken);

        if (invoice is null)
        {
            return null;
        }

        return new InvoiceDetailsDto(
            invoice.Id,
            invoice.SiteId,
            invoice.FileId,
            invoice.FileName,
            invoice.ContentType,
            invoice.DocumentType.ToString(),
            invoice.ExtractionStatus.ToString(),
            invoice.Created,
            invoice.Lines
                .OrderBy(x => x.LineNumber)
                .Select(x => new InvoiceLineDto(
                    x.Id,
                    x.LineNumber,
                    x.Description,
                    x.Quantity,
                    x.UnitPrice,
                    x.Total))
                .ToArray(),
            invoice.ReviewIssues
                .Select(x => new InvoiceReviewIssueDto(
                    x.Id,
                    x.Code,
                    x.Message,
                    x.IsResolved))
                .ToArray());
    }
}
