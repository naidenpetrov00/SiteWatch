using System.ComponentModel.DataAnnotations;
using Domain.SeedWork;
using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class Camera : BaseAuditableEntity
{
    // ReSharper disable once UnusedMember.Local
    private Camera()
    {
    }

    private Camera(CameraName cameraName)
    {
        CameraName = cameraName;
    }

    public CameraName CameraName { get; private set; } = null!;
    public CameraBrand CameraBrand { get; private set; } = null!;
    public string? Username { get; private set; }
    public string? Password { get; private set; }
    public string? IpAddress { get; private set; }
    public int Port { get; private set; } = 554;
    public Site? Site { get; private set; }

    public static Camera Create(string name) => new(name);

    public void AddToSite(Site site) => Site = site;
    public void RemoveFromSite() => Site = null;

    public void UpdateIpAddress(string ipAddress) => IpAddress = ipAddress;
    public void UpdatePort(int port) => Port = port;

    public void UpdateUsername(string username) => Username = username;
    public void UpdatePassword(string password) => Password = password;
}