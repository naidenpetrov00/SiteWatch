namespace Application.SeedWork.Models.External;

public sealed record InvoiceExtractionIssueResult
{
    public string FieldPath { get; init; } = string.Empty;

    public string? ExtractedValue { get; init; }

    public string Reason { get; init; } = string.Empty;

    public decimal? Confidence { get; init; }
}
