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
    public async Task<InvoiceDetailsDto?> Handle(
        GetInvoiceByIdQuery request,
        CancellationToken cancellationToken)
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

        var invoiceDocument = new InvoiceDocumentDto(
            invoice.Id,
            invoice.OriginalFileName,
            invoice.StoredFilePath,
            invoice.Status.ToString(),
            invoice.DocumentType.ToString(),
            invoice.SupplierName,
            invoice.SupplierEik,
            invoice.SupplierVatNumber,
            invoice.BuyerName,
            invoice.InvoiceNumber,
            invoice.InvoiceDate,
            invoice.Currency,
            invoice.NetTotal,
            invoice.VatTotal,
            invoice.GrossTotal,
            invoice.OverallConfidence,
            invoice.RawExtractionJson,
            invoice.CreatedAt,
            invoice.ProcessedAt,
            invoice.ApprovedAt);

        var lines = invoice.Lines
            .OrderBy(x => x.Id)
            .Select(x => new InvoiceLineDto(
                x.Id,
                x.ProductCode,
                x.ProductName,
                x.Quantity,
                x.Unit,
                x.UnitPrice,
                x.Discount,
                x.VatRate,
                x.LineTotal,
                x.Confidence))
            .ToArray();

        var reviewIssues = invoice.ReviewIssues
            .OrderBy(x => x.Id)
            .Select(x => new InvoiceReviewIssueDto(
                x.Id,
                x.FieldPath,
                x.ExtractedValue,
                x.Reason,
                x.Confidence,
                x.CorrectedValue,
                x.Resolved))
            .ToArray();

        return new InvoiceDetailsDto(invoiceDocument, lines, reviewIssues);
    }
}
