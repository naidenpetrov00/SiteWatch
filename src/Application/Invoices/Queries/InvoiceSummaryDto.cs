namespace Application.Invoices.Queries;

public sealed record InvoiceSummaryDto(
    Guid InvoiceId,
    string OriginalFileName,
    string Status,
    string DocumentType,
    string? SupplierName,
    string? InvoiceNumber,
    DateTimeOffset? InvoiceDate,
    string? Currency,
    decimal? GrossTotal,
    decimal? OverallConfidence,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ProcessedAt,
    DateTimeOffset? ApprovedAt);
