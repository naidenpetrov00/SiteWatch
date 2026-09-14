using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace Api.Services;

internal sealed class IssueAttachmentAccessTicketService
    : IIssueAttachmentAccessTicketService
{
    private const string Purpose = "SiteWatch.IssueAttachmentAccess.v1";
    private readonly ITimeLimitedDataProtector _protector;

    public IssueAttachmentAccessTicketService(IDataProtectionProvider provider)
    {
        _protector = provider
            .CreateProtector(Purpose)
            .ToTimeLimitedDataProtector();
    }

    public string Create(
        Guid issueId,
        Guid attachmentId,
        string userId,
        bool preview,
        bool download,
        DateTimeOffset expiresAt)
    {
        var payload = JsonSerializer.Serialize(new IssueAttachmentAccessTicket(
            issueId,
            attachmentId,
            userId,
            preview,
            download));
        return _protector.Protect(payload, expiresAt);
    }

    public bool TryRead(string ticket, out IssueAttachmentAccessTicket? accessTicket)
    {
        accessTicket = null;
        if (string.IsNullOrWhiteSpace(ticket))
        {
            return false;
        }

        try
        {
            var payload = _protector.Unprotect(ticket, out _);
            accessTicket = JsonSerializer.Deserialize<IssueAttachmentAccessTicket>(payload);
            return accessTicket is not null
                   && accessTicket.IssueId != Guid.Empty
                   && accessTicket.AttachmentId != Guid.Empty
                   && !string.IsNullOrWhiteSpace(accessTicket.UserId);
        }
        catch (Exception exception) when (
            exception is CryptographicException or JsonException or FormatException)
        {
            return false;
        }
    }
}
