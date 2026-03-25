using Domain.SeedWork;
using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class Camera : BaseAuditableEntity
{
    public CameraName CameraName { get; private set; } = null!;
    public CameraBrand CameraBrand { get; private set; } = null!;
    public string? Username { get; private set; }
    public string? Password { get; private set; }
    public string? IpAddress { get; private set; }
    public int? RtspPort { get; private set; } = 554;
    public int? PtzPort { get; private set; } = 443;
    public Guid? SiteId { get; init; }
    public Site? Site { get; private set; }

    // ReSharper disable once UnusedMember.Local
    private Camera()
    {
    }

    private Camera(CameraName cameraName, CameraBrand cameraBrand)
    {
        CameraName = cameraName;
        CameraBrand = cameraBrand;
    }

    public static Camera Create(
        CameraName cameraName,
        CameraBrand cameraBrand,
        string? username = null,
        string? password = null,
        string? ipAddress = null,
        int? port = null,
        Guid? siteId = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            CameraName = cameraName,
            CameraBrand = cameraBrand,
            Username = username,
            Password = password,
            IpAddress = ipAddress,
            RtspPort = port,
            SiteId = siteId
        };

    public void AddToSite(Site site) => Site = site;
    public void RemoveFromSite() => Site = null;

    public void UpdateIpAddress(string? ipAddress) => IpAddress = ipAddress;
    public void UpdateRtspPort(int port) => RtspPort = port;
    public void UpdatePtzPort(int port) => RtspPort = port;

    public void UpdateUsername(string username) => Username = username;
    public void UpdatePassword(string password) => Password = password;
}