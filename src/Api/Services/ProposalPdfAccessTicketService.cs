using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace Api.Services;

internal sealed record ProposalPdfAccessTicket(
    Guid SiteId,
    Guid ProposalId,
    string UserId);

internal interface IProposalPdfAccessTicketService
{
    string Create(
        Guid siteId,
        Guid proposalId,
        string userId,
        DateTimeOffset expiresAt);

    bool TryRead(string ticket, out ProposalPdfAccessTicket? accessTicket);
}

internal sealed class ProposalPdfAccessTicketService
    : IProposalPdfAccessTicketService
{
    private const string Purpose = "SiteWatch.ProposalPdfAccess.v1";
    private readonly ITimeLimitedDataProtector _protector;

    public ProposalPdfAccessTicketService(IDataProtectionProvider provider)
    {
        _protector = provider
            .CreateProtector(Purpose)
            .ToTimeLimitedDataProtector();
    }

    public string Create(
        Guid siteId,
        Guid proposalId,
        string userId,
        DateTimeOffset expiresAt)
    {
        var payload = JsonSerializer.Serialize(
            new ProposalPdfAccessTicket(siteId, proposalId, userId));
        return _protector.Protect(payload, expiresAt);
    }

    public bool TryRead(
        string ticket,
        out ProposalPdfAccessTicket? accessTicket)
    {
        accessTicket = null;
        if (string.IsNullOrWhiteSpace(ticket))
        {
            return false;
        }

        try
        {
            var payload = _protector.Unprotect(ticket, out _);
            accessTicket = JsonSerializer.Deserialize<ProposalPdfAccessTicket>(payload);
            return accessTicket is not null
                && accessTicket.SiteId != Guid.Empty
                && accessTicket.ProposalId != Guid.Empty
                && !string.IsNullOrWhiteSpace(accessTicket.UserId);
        }
        catch (Exception exception) when (
            exception is CryptographicException or JsonException or FormatException)
        {
            return false;
        }
    }
}

internal sealed record ClientProposalPdfAccessTicket(
    Guid SiteId,
    Guid ProposalId,
    string UserId);

internal interface IClientProposalPdfAccessTicketService
{
    string Create(
        Guid siteId,
        Guid proposalId,
        string userId,
        DateTimeOffset expiresAt);

    bool TryRead(
        string ticket,
        out ClientProposalPdfAccessTicket? accessTicket);
}

internal sealed class ClientProposalPdfAccessTicketService
    : IClientProposalPdfAccessTicketService
{
    private const string Purpose = "SiteWatch.ClientProposalPdfAccess.v1";
    private readonly ITimeLimitedDataProtector _protector;

    public ClientProposalPdfAccessTicketService(IDataProtectionProvider provider)
    {
        _protector = provider
            .CreateProtector(Purpose)
            .ToTimeLimitedDataProtector();
    }

    public string Create(
        Guid siteId,
        Guid proposalId,
        string userId,
        DateTimeOffset expiresAt)
    {
        var payload = JsonSerializer.Serialize(
            new ClientProposalPdfAccessTicket(siteId, proposalId, userId));
        return _protector.Protect(payload, expiresAt);
    }

    public bool TryRead(
        string ticket,
        out ClientProposalPdfAccessTicket? accessTicket)
    {
        accessTicket = null;
        if (string.IsNullOrWhiteSpace(ticket))
        {
            return false;
        }

        try
        {
            var payload = _protector.Unprotect(ticket, out _);
            accessTicket = JsonSerializer.Deserialize<ClientProposalPdfAccessTicket>(payload);
            return accessTicket is not null
                && accessTicket.SiteId != Guid.Empty
                && accessTicket.ProposalId != Guid.Empty
                && !string.IsNullOrWhiteSpace(accessTicket.UserId);
        }
        catch (Exception exception) when (
            exception is CryptographicException or JsonException or FormatException)
        {
            return false;
        }
    }
}
