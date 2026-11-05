using Domain.SeedWork.Enums;

namespace Application.Persons.Commands;

public abstract record PersonUpsertDto
{
    public PersonType? Type { get; init; }
    public string? FirstName { get; init; }
    public string? MiddleName { get; init; }
    public string? LastName { get; init; }
    public string? CompanyName { get; init; }
    public string? LegalForm { get; init; }
    public string? Egn { get; init; }
    public string? Eik { get; init; }
    public string? VatNumber { get; init; }
    public List<PersonAddressDto>? Addresses { get; init; }
    public List<PersonContactDto>? Contacts { get; init; }
    public List<PersonBankAccountDto>? BankAccounts { get; init; }
}

public sealed record PersonAddressDto : IPrimaryState
{
    public string Country { get; init; } = "Bulgaria";
    public Guid? MunicipalityId { get; init; }
    public Guid? SettlementId { get; init; }
    public Guid? DistrictId { get; init; }
    public Guid? QuarterId { get; init; }
    public string? MunicipalityName { get; init; }
    public string? SettlementName { get; init; }
    public string? DistrictName { get; init; }
    public string? QuarterName { get; init; }
    public string? PostalCode { get; init; }
    public string? Street { get; init; }
    public string? Number { get; init; }
    public string? Building { get; init; }
    public string? Entrance { get; init; }
    public string? Floor { get; init; }
    public string? Apartment { get; init; }
    public string? AddressDetails { get; init; }
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
