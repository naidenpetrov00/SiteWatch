using Domain.SeedWork;

namespace Domain.Entities;

public sealed class InvoiceReviewIssue : BaseEntity
{
    private InvoiceReviewIssue()
    {
    }

    public Guid InvoiceDocumentId { get; private set; }
    public string FieldPath { get; private set; } = null!;
    public string? ExtractedValue { get; private set; }
    public string Reason { get; private set; } = null!;
    public decimal? Confidence { get; private set; }
    public string? CorrectedValue { get; private set; }
    public bool Resolved { get; private set; }

    public InvoiceDocument InvoiceDocument { get; private set; } = null!;

    public static InvoiceReviewIssue Create(
        Guid invoiceDocumentId,
        string fieldPath,
        string? extractedValue,
        string reason,
        decimal? confidence,
        string? correctedValue = null,
        bool resolved = false)
        => new()
        {
            Id = Guid.NewGuid(),
            InvoiceDocumentId = invoiceDocumentId,
            FieldPath = fieldPath,
            ExtractedValue = extractedValue,
            Reason = reason,
            Confidence = confidence,
            CorrectedValue = correctedValue,
            Resolved = resolved
        };
}
