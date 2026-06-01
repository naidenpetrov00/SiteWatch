namespace Application.Invoices.Queries;

public sealed record InvoiceDocumentDto(
    Guid Id,
    string OriginalFileName,
    string StoredFilePath,
    string Status,
    string DocumentType,
    string? SupplierName,
    string? SupplierEik,
    string? SupplierVatNumber,
    string? BuyerName,
    string? InvoiceNumber,
    DateTimeOffset? InvoiceDate,
    string? Currency,
    decimal? NetTotal,
    decimal? VatTotal,
    decimal? GrossTotal,
    decimal? OverallConfidence,
    string? RawExtractionJson,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ProcessedAt,
    DateTimeOffset? ApprovedAt);
