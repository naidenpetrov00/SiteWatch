namespace Api.Services;

internal sealed record IssueAttachmentAccessTicket(
    Guid IssueId,
    Guid AttachmentId,
    string UserId,
    bool Preview,
    bool Download);
