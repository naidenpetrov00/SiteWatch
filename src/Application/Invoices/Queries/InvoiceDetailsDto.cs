namespace Application.Invoices.Queries;

public sealed record InvoiceDetailsDto(
    InvoiceDocumentDto InvoiceDocument,
    IReadOnlyCollection<InvoiceLineDto> InvoiceLines,
    IReadOnlyCollection<InvoiceReviewIssueDto> InvoiceReviewIssues);
