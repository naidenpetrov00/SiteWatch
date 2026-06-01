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

    public void MarkProcessing() => Status = InvoiceExtractionStatus.Processing;

    public void MarkFailed() => Status = InvoiceExtractionStatus.Failed;

    public void ApplyExtractionFields(
        InvoiceDocumentType documentType,
        string? supplierName,
        string? supplierEik,
        string? supplierVatNumber,
        string? buyerName,
        string? invoiceNumber,
        DateTimeOffset? invoiceDate,
        string? currency,
        decimal? netTotal,
        decimal? vatTotal,
        decimal? grossTotal,
        decimal? overallConfidence)
    {
        DocumentType = documentType;
        SupplierName = supplierName;
        SupplierEik = supplierEik;
        SupplierVatNumber = supplierVatNumber;
        BuyerName = buyerName;
        InvoiceNumber = invoiceNumber;
        InvoiceDate = invoiceDate;
        Currency = currency;
        NetTotal = netTotal;
        VatTotal = vatTotal;
        GrossTotal = grossTotal;
        OverallConfidence = overallConfidence;
    }

    public void CompleteProcessing(
        InvoiceExtractionStatus status,
        DateTimeOffset processedAt,
        string? rawExtractionJson)
    {
        Status = status;
        ProcessedAt = processedAt;
        RawExtractionJson = rawExtractionJson;
    }

    public void ReplaceLines(IEnumerable<InvoiceLine> lines)
    {
        Lines.Clear();
        Lines.AddRange(lines);
    }

    public void ReplaceReviewIssues(IEnumerable<InvoiceReviewIssue> reviewIssues)
    {
        ReviewIssues.Clear();
        ReviewIssues.AddRange(reviewIssues);
    }

    public void Approve(DateTimeOffset approvedAt)
    {
        Status = InvoiceExtractionStatus.Approved;
        ApprovedAt = approvedAt;
    }
}
