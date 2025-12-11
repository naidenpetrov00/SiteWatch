using Ardalis.GuardClauses;
using Domain.SeedWork;

namespace Domain.ValueObjects;

public class CameraName : ValueObject
{
    public string Value { get; }

    private CameraName(string value)
    {
        Guard.Against.NullOrWhiteSpace(value, nameof(value));
        Guard.Against.OutOfRange(value.Length, nameof(value), 1, 100);

        Value = value.Trim();
    }

    private static CameraName Create(string name) => new(name);

    public override string ToString() => Value;

    public static implicit operator string(CameraName cameraName) => cameraName.ToString();
    public static implicit operator CameraName(string value) => Create(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}