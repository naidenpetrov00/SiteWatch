namespace Application.SeedWork.Models.External;

public sealed record InvoiceValidationResult(
    bool IsValidForAutoApproval,
    List<InvoiceValidationIssueResult> Issues);
