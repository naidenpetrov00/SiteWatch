using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace Api.Services;

internal sealed record InvoiceFileAccessTicket(
    Guid SiteId,
    Guid InvoiceId,
    string UserId);

internal interface IInvoiceFileAccessTicketService
{
    string Create(
        Guid siteId,
        Guid invoiceId,
        string userId,
        DateTimeOffset expiresAt);

    bool TryRead(string ticket, out InvoiceFileAccessTicket? accessTicket);
}

internal sealed class InvoiceFileAccessTicketService
    : IInvoiceFileAccessTicketService
{
    private const string Purpose = "SiteWatch.InvoiceFileAccess.v1";
    private readonly ITimeLimitedDataProtector _protector;

    public InvoiceFileAccessTicketService(IDataProtectionProvider provider)
    {
        _protector = provider
            .CreateProtector(Purpose)
            .ToTimeLimitedDataProtector();
    }

    public string Create(
        Guid siteId,
        Guid invoiceId,
        string userId,
        DateTimeOffset expiresAt)
    {
        var payload = JsonSerializer.Serialize(
            new InvoiceFileAccessTicket(siteId, invoiceId, userId));

        return _protector.Protect(payload, expiresAt);
    }

    public bool TryRead(
        string ticket,
        out InvoiceFileAccessTicket? accessTicket)
    {
        accessTicket = null;

        if (string.IsNullOrWhiteSpace(ticket))
        {
            return false;
        }

        try
        {
            var payload = _protector.Unprotect(ticket, out _);
            accessTicket = JsonSerializer.Deserialize<InvoiceFileAccessTicket>(payload);

            return accessTicket is not null
                   && accessTicket.SiteId != Guid.Empty
                   && accessTicket.InvoiceId != Guid.Empty
                   && !string.IsNullOrWhiteSpace(accessTicket.UserId);
        }
        catch (Exception exception) when (
            exception is CryptographicException or JsonException or FormatException)
        {
            return false;
        }
    }
}
