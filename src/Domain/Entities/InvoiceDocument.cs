using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class InvoiceDocument : BaseAuditableEntity
{
    private InvoiceDocument()
    {
    }

    public Guid SiteId { get; private set; }
    public Guid FileId { get; private set; }
    public string FileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public InvoiceDocumentType DocumentType { get; private set; }
    public InvoiceExtractionStatus ExtractionStatus { get; private set; }

    public List<InvoiceLine> Lines { get; private set; } = [];
    public List<InvoiceReviewIssue> ReviewIssues { get; private set; } = [];
    public Site Site { get; private set; } = null!;

    public static InvoiceDocument Create(
        Guid siteId,
        Guid fileId,
        string fileName,
        string contentType,
        InvoiceDocumentType documentType)
        => new()
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            FileId = fileId,
            FileName = fileName,
            ContentType = contentType,
            DocumentType = documentType,
            ExtractionStatus = InvoiceExtractionStatus.Uploaded
        };
}
