using Ardalis.GuardClauses;
using Domain.SeedWork;

namespace Domain.Entities;

public sealed class PersonAddress : BaseAuditableEntity
{
    private PersonAddress()
    {
    }

    public static PersonAddress Create(
        Guid personId,
        string addressLine,
        string? city = null,
        string? postalCode = null,
        string? country = null,
        string? additionalLine = null,
        string? details = null,
        bool isPrimary = false,
        bool isActive = true)
    {
        return new PersonAddress
        {
            Id = Guid.NewGuid(),
            PersonId = personId,
            AddressLine = Guard.Against.NullOrWhiteSpace(addressLine, nameof(addressLine)).Trim(),
            City = string.IsNullOrWhiteSpace(city) ? null : city.Trim(),
            PostalCode = string.IsNullOrWhiteSpace(postalCode) ? null : postalCode.Trim(),
            Country = string.IsNullOrWhiteSpace(country) ? null : country.Trim(),
            AdditionalLine = string.IsNullOrWhiteSpace(additionalLine) ? null : additionalLine.Trim(),
            Details = string.IsNullOrWhiteSpace(details) ? null : details.Trim(),
            IsPrimary = isPrimary,
            IsActive = isActive
        };
    }

    public Guid PersonId { get; private set; }
    public string AddressLine { get; private set; } = null!;
    public string? AdditionalLine { get; private set; }
    public string? City { get; private set; }
    public string? PostalCode { get; private set; }
    public string? Country { get; private set; }
    public string? Details { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsPrimary { get; private set; }

    public Person Person { get; private set; } = null!;

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public void MarkPrimary() => IsPrimary = true;

    public void UnmarkPrimary() => IsPrimary = false;
}
