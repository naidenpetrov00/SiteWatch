using Ardalis.GuardClauses;
using Domain.SeedWork;

namespace Domain.ValueObjects;

public sealed class SiteName : ValueObject
{
    public string Value { get; }

    private SiteName(string value)
    {
        Guard.Against.NullOrWhiteSpace(value, nameof(value), "Site name cannot be empty.");
        Guard.Against.OutOfRange(
            value.Length,
            nameof(value),
            5,
            100,
            "Site name must be between 3 and 100 characters."
        );

        Value = value.Trim();
    }

    private static SiteName Create(string value) => new(value);

    public static implicit operator string(SiteName name) => name.ToString();

    public static implicit operator SiteName(string name) => Create(name);

    public override string ToString() => Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}