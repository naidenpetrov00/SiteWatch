using Domain.Entities;

namespace Application.Cameras.Streaming;

public enum CameraStreamProfile
{
    Main,
    Sub,
}

public static class CameraStreamProfiles
{
    public static bool TryParse(string? value, out CameraStreamProfile profile)
    {
        if (string.Equals(value?.Trim(), "main", StringComparison.OrdinalIgnoreCase))
        {
            profile = CameraStreamProfile.Main;
            return true;
        }

        if (string.Equals(value?.Trim(), "sub", StringComparison.OrdinalIgnoreCase))
        {
            profile = CameraStreamProfile.Sub;
            return true;
        }

        profile = default;
        return false;
    }
}

public static class CameraStreamPaths
{
    public static string Create(Guid cameraId, CameraStreamProfile profile) =>
        $"camera-{cameraId:N}-{ToSegment(profile)}";

    public static bool IsValidForCamera(string path, Guid cameraId) =>
        string.Equals(
            path,
            Create(cameraId, CameraStreamProfile.Main),
            StringComparison.Ordinal)
        || string.Equals(
            path,
            Create(cameraId, CameraStreamProfile.Sub),
            StringComparison.Ordinal);

    private static string ToSegment(CameraStreamProfile profile) => profile switch
    {
        CameraStreamProfile.Main => "main",
        CameraStreamProfile.Sub => "sub",
        _ => throw new ArgumentOutOfRangeException(nameof(profile)),
    };
}

public sealed record PreparedCameraStream(string Path, Uri WhepUrl);

public interface IMediaMtxGateway
{
    Task<PreparedCameraStream> ReconcileCameraPathAsync(
        Camera camera,
        CameraStreamProfile profile,
        CancellationToken cancellationToken);
}

public sealed record MediaMtxViewerReadTicket(
    Guid SiteId,
    Guid CameraId,
    string Path,
    string UserId,
    DateTimeOffset ExpiresAt);

public sealed record MediaMtxControlApiTicket(
    Guid Nonce,
    DateTimeOffset ExpiresAt);

public sealed class IssuedMediaMtxViewerReadTicket(
    string ticket,
    DateTimeOffset expiresAt)
{
    public string Ticket { get; } = ticket;
    public DateTimeOffset ExpiresAt { get; } = expiresAt;
}

public interface IMediaMtxAccessTicketService
{
    IssuedMediaMtxViewerReadTicket CreateViewerReadTicket(
        Guid siteId,
        Guid cameraId,
        string path,
        string userId);

    bool TryReadViewerReadTicket(
        string ticket,
        out MediaMtxViewerReadTicket? accessTicket);

    string CreateControlApiTicket();

    bool TryReadControlApiTicket(string ticket);
}
