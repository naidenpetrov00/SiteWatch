namespace Application.SeedWork.Models.External;

public sealed record InvoiceValidationIssueResult(
    string FieldPath,
    string? ExtractedValue,
    string Reason,
    decimal? Confidence);
