using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class InvoiceDocument : BaseEntity
{
    private InvoiceDocument()
    {
    }

    public Guid SiteId { get; private set; }
    public string OriginalFileName { get; private set; } = null!;
    public string StoredFilePath { get; private set; } = null!;
    public InvoiceExtractionStatus Status { get; private set; }
    public InvoiceDocumentType DocumentType { get; private set; }
    public string? SupplierName { get; private set; }
    public string? SupplierEik { get; private set; }
    public string? SupplierVatNumber { get; private set; }
    public string? BuyerName { get; private set; }
    public string? InvoiceNumber { get; private set; }
    public DateTimeOffset? InvoiceDate { get; private set; }
    public string? Currency { get; private set; }
    public decimal? NetTotal { get; private set; }
    public decimal? VatTotal { get; private set; }
    public decimal? GrossTotal { get; private set; }
    public decimal? OverallConfidence { get; private set; }
    public string? RawExtractionJson { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }

    public List<InvoiceLine> Lines { get; private set; } = [];
    public List<InvoiceReviewIssue> ReviewIssues { get; private set; } = [];

    public static InvoiceDocument Create(
        Guid siteId,
        string originalFileName,
        string storedFilePath,
        InvoiceDocumentType documentType)
        => new()
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            OriginalFileName = originalFileName,
            StoredFilePath = storedFilePath,
            DocumentType = documentType,
            Status = InvoiceExtractionStatus.Uploaded,
            CreatedAt = DateTimeOffset.UtcNow
        };

    public void UpdateExtractionResult(
        InvoiceExtractionStatus status,
        DateTimeOffset processedAt,
        string? rawExtractionJson = null)
    {
        Status = status;
        ProcessedAt = processedAt;
        RawExtractionJson = rawExtractionJson;
    }

    public void Approve(DateTimeOffset approvedAt)
    {
        Status = InvoiceExtractionStatus.Approved;
        ApprovedAt = approvedAt;
    }
}
