namespace Application.Invoices.Queries;

public sealed record InvoiceSummaryDto(
    Guid InvoiceId,
    Guid SiteId,
    Guid FileId,
    string FileName,
    string ContentType,
    string DocumentType,
    string ExtractionStatus,
    DateTimeOffset Created);
