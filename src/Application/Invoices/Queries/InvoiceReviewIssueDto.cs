namespace Application.Invoices.Queries;

public sealed record InvoiceReviewIssueDto(
    Guid Id,
    string Code,
    string Message,
    bool IsResolved);
