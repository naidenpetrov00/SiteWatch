using Ardalis.GuardClauses;
using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class PersonContact : BaseAuditableEntity
{
    private PersonContact()
    {
    }

    public static PersonContact Create(
        Guid personId,
        ContactType contactType,
        string value,
        string? details = null,
        bool isPrimary = false,
        bool isActive = true)
    {
        return new PersonContact
        {
            Id = Guid.NewGuid(),
            PersonId = personId,
            ContactType = contactType,
            Value = Guard.Against.NullOrWhiteSpace(value, nameof(value)).Trim(),
            Details = string.IsNullOrWhiteSpace(details) ? null : details.Trim(),
            IsPrimary = isPrimary,
            IsActive = isActive
        };
    }

    public Guid PersonId { get; private set; }
    public ContactType ContactType { get; private set; }
    public string Value { get; private set; } = null!;
    public string? Details { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsPrimary { get; private set; }

    public Person Person { get; private set; } = null!;

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public void MarkPrimary() => IsPrimary = true;

    public void UnmarkPrimary() => IsPrimary = false;
}
