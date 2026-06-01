namespace Application.Invoices.Queries;

public sealed record InvoiceSummaryDto(
    Guid Id,
    string OriginalFileName,
    string Status,
    string DocumentType,
    string? SupplierName,
    string? InvoiceNumber,
    DateTimeOffset? InvoiceDate,
    decimal? GrossTotal,
    DateTimeOffset CreatedAt);
