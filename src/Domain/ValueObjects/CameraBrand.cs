using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.SeedWork;

namespace Domain.ValueObjects;

public class CameraBrand : ValueObject
{
    public string Brand { get; }
    public string Model { get; }

    private CameraBrand(string brand, string model)
    {
        Guard.Against.NullOrWhiteSpace(brand);
        Guard.Against.NullOrWhiteSpace(model);
        Guard.Against.OutOfRange(brand.Length, nameof(brand), 2, 20);
        Guard.Against.OutOfRange(model.Length, nameof(model), 2, 100);

        Brand = brand;
        Model = model;
    }

    public static CameraBrand Create(string brand, string model) => new(brand, model);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Brand;
        yield return Model;
    }

    protected string BuildRtspUrl(Camera camera)
    {
        return
            $"rtsp://{camera.Username}:{camera.Password}@{camera.IpAddress}:{camera.Port}/cam/realmonitor?channel=1&subtype=0";
    }
}