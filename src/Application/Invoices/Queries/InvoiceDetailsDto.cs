namespace Application.Invoices.Queries;

public sealed record InvoiceDetailsDto(
    Guid InvoiceId,
    Guid SiteId,
    Guid FileId,
    string FileName,
    string ContentType,
    string DocumentType,
    string ExtractionStatus,
    DateTimeOffset Created,
    IReadOnlyCollection<InvoiceLineDto> Lines,
    IReadOnlyCollection<InvoiceReviewIssueDto> ReviewIssues);
