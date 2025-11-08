using Ardalis.GuardClauses;
using Domain.SeedWork;

namespace Domain.ValueObjects;

public sealed class SiteAddress : ValueObject
{
    public string Value { get; }

    private SiteAddress(string value)
    {
        Guard.Against.NullOrWhiteSpace(value, nameof(value), "Address cannot be empty.");
        Guard.Against.OutOfRange(
            value.Length,
            nameof(value),
            5,
            200,
            "Address must be between 5 and 200 characters."
        );

        Value = value.Trim();
    }

    public static SiteAddress Create(string value) => new(value);

    public static implicit operator string(SiteAddress address) => address.ToString();

    public static implicit operator SiteAddress(string address) => Create(address);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
