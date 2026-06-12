using Ardalis.GuardClauses;
using Domain.SeedWork;

namespace Domain.Entities;

public sealed class PersonBankAccount : BaseAuditableEntity
{
    private PersonBankAccount()
    {
    }

    public static PersonBankAccount Create(
        Guid personId,
        string iban,
        string? bic = null,
        string? bankName = null,
        string? details = null,
        bool isPrimary = false,
        bool isActive = true)
    {
        return new PersonBankAccount
        {
            Id = Guid.NewGuid(),
            PersonId = personId,
            IBAN = Guard.Against.NullOrWhiteSpace(iban, nameof(iban)).Trim().Replace(" ", string.Empty),
            BIC = string.IsNullOrWhiteSpace(bic) ? null : bic.Trim(),
            BankName = string.IsNullOrWhiteSpace(bankName) ? null : bankName.Trim(),
            Details = string.IsNullOrWhiteSpace(details) ? null : details.Trim(),
            IsPrimary = isPrimary,
            IsActive = isActive
        };
    }

    public Guid PersonId { get; private set; }
    public string IBAN { get; private set; } = null!;
    public string? BIC { get; private set; }
    public string? BankName { get; private set; }
    public string? Details { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsPrimary { get; private set; }

    public Person Person { get; private set; } = null!;

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public void MarkPrimary() => IsPrimary = true;

    public void UnmarkPrimary() => IsPrimary = false;
}
