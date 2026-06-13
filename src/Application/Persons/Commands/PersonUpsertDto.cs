using Domain.SeedWork.Enums;

namespace Application.Persons.Commands;

public abstract record PersonUpsertDto
{
    public PersonType? Type { get; init; }
    public string? FirstName { get; init; }
    public string? MiddleName { get; init; }
    public string? LastName { get; init; }
    public string? CompanyName { get; init; }
    public string? Egn { get; init; }
    public string? Eik { get; init; }
    public List<PersonAddressDto>? Addresses { get; init; }
    public List<PersonContactDto>? Contacts { get; init; }
    public List<PersonBankAccountDto>? BankAccounts { get; init; }
}

public sealed record PersonAddressDto : IPrimaryState
{
    public string AddressLine { get; init; } = string.Empty;
    public string? AdditionalLine { get; init; }
    public string? City { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    public string? Details { get; init; }
    public bool IsActive { get; init; } = true;
    public bool IsPrimary { get; init; }
}

public sealed record PersonContactDto : IPrimaryState
{
    public Domain.SeedWork.Enums.ContactType ContactType { get; init; }
    public string Value { get; init; } = string.Empty;
    public string? Details { get; init; }
    public bool IsActive { get; init; } = true;
    public bool IsPrimary { get; init; }
}

public sealed record PersonBankAccountDto : IPrimaryState
{
    public string IBAN { get; init; } = string.Empty;
    public string? BIC { get; init; }
    public string? BankName { get; init; }
    public string? Details { get; init; }
    public bool IsActive { get; init; } = true;
    public bool IsPrimary { get; init; }
}
