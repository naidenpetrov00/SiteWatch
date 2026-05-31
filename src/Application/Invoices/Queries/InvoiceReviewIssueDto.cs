namespace Application.Invoices.Queries;

public sealed record InvoiceReviewIssueDto(
    Guid Id,
    string FieldPath,
    string? ExtractedValue,
    string Reason,
    decimal? Confidence,
    string? CorrectedValue,
    bool Resolved);
