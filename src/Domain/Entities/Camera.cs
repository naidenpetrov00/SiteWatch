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
    public int Port { get; private set; } = 554;
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

    public static Camera Create(string name, CameraBrand cameraBrand) => new(name, cameraBrand);

    public void AddToSite(Site site) => Site = site;
    public void RemoveFromSite() => Site = null;

    public void UpdateIpAddress(string ipAddress) => IpAddress = ipAddress;
    public void UpdatePort(int port) => Port = port;

    public void UpdateUsername(string username) => Username = username;
    public void UpdatePassword(string password) => Password = password;
}