using Domain.SeedWork;

namespace Domain.Entities;

public sealed class PersonAddress : BaseAuditableEntity
{
    private PersonAddress()
    {
    }

    public static PersonAddress Create(
        Guid personId,
        string? country = null,
        string? municipality = null,
        string? settlement = null,
        string? district = null,
        string? quarter = null,
        string? postalCode = null,
        string? street = null,
        string? number = null,
        string? building = null,
        string? entrance = null,
        string? floor = null,
        string? apartment = null,
        string? addressDetails = null,
        bool isPrimary = false,
        bool isActive = true)
    {
        return new PersonAddress
        {
            Id = Guid.NewGuid(),
            PersonId = personId,
            Country = string.IsNullOrWhiteSpace(country) ? "Bulgaria" : country.Trim(),
            Municipality = Normalize(municipality),
            Settlement = Normalize(settlement),
            District = Normalize(district),
            Quarter = Normalize(quarter),
            PostalCode = Normalize(postalCode),
            Street = Normalize(street),
            Number = Normalize(number),
            Building = Normalize(building),
            Entrance = Normalize(entrance),
            Floor = Normalize(floor),
            Apartment = Normalize(apartment),
            AddressDetails = Normalize(addressDetails),
            IsPrimary = isPrimary,
            IsActive = isActive
        };
    }

    public Guid PersonId { get; private set; }
    public string Country { get; private set; } = null!;
    public string? Municipality { get; private set; }
    public string? Settlement { get; private set; }
    public string? District { get; private set; }
    public string? Quarter { get; private set; }
    public string? PostalCode { get; private set; }
    public string? Street { get; private set; }
    public string? Number { get; private set; }
    public string? Building { get; private set; }
    public string? Entrance { get; private set; }
    public string? Floor { get; private set; }
    public string? Apartment { get; private set; }
    public string? AddressDetails { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsPrimary { get; private set; }

    public Person Person { get; private set; } = null!;

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public void MarkPrimary() => IsPrimary = true;

    public void UnmarkPrimary() => IsPrimary = false;

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
