using System.Security.Cryptography;
using System.Text.Json;
using Application.Cameras.Streaming;
using Microsoft.AspNetCore.DataProtection;

namespace Api.Services;

internal sealed class MediaMtxAccessTicketService : IMediaMtxAccessTicketService
{
    private const string ViewerReadPurpose = "SiteWatch.MediaMtx.ViewerRead.v1";
    private const string ControlApiPurpose = "SiteWatch.MediaMtx.ControlApi.v1";
    private static readonly TimeSpan ViewerReadLifetime = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan ControlApiLifetime = TimeSpan.FromSeconds(30);

    private readonly ITimeLimitedDataProtector _viewerReadProtector;
    private readonly ITimeLimitedDataProtector _controlApiProtector;
    private readonly TimeProvider _timeProvider;

    public MediaMtxAccessTicketService(
        IDataProtectionProvider provider,
        TimeProvider timeProvider)
    {
        _viewerReadProtector = provider
            .CreateProtector(ViewerReadPurpose)
            .ToTimeLimitedDataProtector();
        _controlApiProtector = provider
            .CreateProtector(ControlApiPurpose)
            .ToTimeLimitedDataProtector();
        _timeProvider = timeProvider;
    }

    public IssuedMediaMtxViewerReadTicket CreateViewerReadTicket(
        Guid siteId,
        Guid cameraId,
        string path,
        string userId)
    {
        var expiresAt = _timeProvider.GetUtcNow().Add(ViewerReadLifetime);
        var payload = JsonSerializer.Serialize(new MediaMtxViewerReadTicket(
            siteId,
            cameraId,
            path,
            userId,
            expiresAt));
        var ticket = _viewerReadProtector.Protect(payload, expiresAt);

        return new IssuedMediaMtxViewerReadTicket(ticket, expiresAt);
    }

    public bool TryReadViewerReadTicket(
        string ticket,
        out MediaMtxViewerReadTicket? accessTicket)
    {
        accessTicket = null;
        if (string.IsNullOrWhiteSpace(ticket))
        {
            return false;
        }

        try
        {
            var payload = _viewerReadProtector.Unprotect(ticket, out var protectedExpiresAt);
            var candidate = JsonSerializer.Deserialize<MediaMtxViewerReadTicket>(payload);
            if (candidate is null
                || candidate.SiteId == Guid.Empty
                || candidate.CameraId == Guid.Empty
                || string.IsNullOrWhiteSpace(candidate.UserId)
                || !CameraStreamPaths.IsValidForCamera(candidate.Path, candidate.CameraId)
                || !ExpiryIsValid(candidate.ExpiresAt, protectedExpiresAt))
            {
                return false;
            }

            accessTicket = candidate;
            return true;
        }
        catch (Exception exception) when (
            exception is CryptographicException or JsonException or FormatException)
        {
            return false;
        }
    }

    public string CreateControlApiTicket()
    {
        var expiresAt = _timeProvider.GetUtcNow().Add(ControlApiLifetime);
        var payload = JsonSerializer.Serialize(new MediaMtxControlApiTicket(
            Guid.NewGuid(),
            expiresAt));

        return _controlApiProtector.Protect(payload, expiresAt);
    }

    public bool TryReadControlApiTicket(string ticket)
    {
        if (string.IsNullOrWhiteSpace(ticket))
        {
            return false;
        }

        try
        {
            var payload = _controlApiProtector.Unprotect(ticket, out var protectedExpiresAt);
            var candidate = JsonSerializer.Deserialize<MediaMtxControlApiTicket>(payload);

            return candidate is not null
                && candidate.Nonce != Guid.Empty
                && ExpiryIsValid(candidate.ExpiresAt, protectedExpiresAt);
        }
        catch (Exception exception) when (
            exception is CryptographicException or JsonException or FormatException)
        {
            return false;
        }
    }

    private bool ExpiryIsValid(
        DateTimeOffset payloadExpiresAt,
        DateTimeOffset protectedExpiresAt) =>
        payloadExpiresAt.ToUniversalTime() == protectedExpiresAt.ToUniversalTime()
        && payloadExpiresAt > _timeProvider.GetUtcNow();
}
