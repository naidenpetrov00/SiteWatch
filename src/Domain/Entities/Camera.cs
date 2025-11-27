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
    public Site? Site { get; private set; }

    public void AddToSite(Site site) => Site = site;
    public void RemoveFromSite() => Site = null;

    public static Camera Create(string name) => new(name);
}