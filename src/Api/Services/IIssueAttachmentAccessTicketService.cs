namespace Api.Services;

internal interface IIssueAttachmentAccessTicketService
{
    string Create(
        Guid issueId,
        Guid attachmentId,
        string userId,
        bool preview,
        bool download,
        DateTimeOffset expiresAt);

    bool TryRead(string ticket, out IssueAttachmentAccessTicket? accessTicket);
}
