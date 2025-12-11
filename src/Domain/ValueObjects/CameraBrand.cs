using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.SeedWork;

namespace Domain.ValueObjects;

public class CameraBrand : ValueObject
{
    public Brand Brand { get; }
    public string Model { get; }

    private CameraBrand(Brand brand, string model)
    {
        Guard.Against.EnumOutOfRange(brand);
        Guard.Against.NullOrWhiteSpace(model);
        Guard.Against.OutOfRange(model.Length, nameof(model), 2, 100);

        Brand = brand;
        Model = model;
    }

    public static CameraBrand Create(Brand brand, string model) => new(brand, model);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Brand;
        yield return Model;
    }
}