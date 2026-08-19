using Domain.Entities;

namespace Application.Cameras.Queries;

public sealed record DashboardCameraDto(
    Guid Id,
    int NumberId,
    string Name,
    string Brand,
    string Model,
    string? IpAddress,
    int? RtspPort,
    int? PtzPort,
    Guid? SiteId,
    string? SiteName)
{
    public static DashboardCameraDto From(Camera camera) => new(
        camera.Id,
        camera.NumberId,
        camera.CameraName.Value,
        camera.CameraBrand.Brand.ToString(),
        camera.CameraBrand.Model,
        camera.IpAddress,
        camera.RtspPort,
        camera.PtzPort,
        camera.SiteId,
        camera.Site?.Name.Value);
}

public sealed record DashboardCameraDetailsDto(
    Guid Id,
    int NumberId,
    string Name,
    string Brand,
    string Model,
    string? Username,
    string? Password,
    string? IpAddress,
    int? RtspPort,
    int? PtzPort,
    Guid? SiteId,
    string? SiteName)
{
    public static DashboardCameraDetailsDto From(Camera camera) => new(
        camera.Id,
        camera.NumberId,
        camera.CameraName.Value,
        camera.CameraBrand.Brand.ToString(),
        camera.CameraBrand.Model,
        camera.Username,
        camera.Password,
        camera.IpAddress,
        camera.RtspPort,
        camera.PtzPort,
        camera.SiteId,
        camera.Site?.Name.Value);
}
