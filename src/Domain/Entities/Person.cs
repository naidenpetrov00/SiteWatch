using Ardalis.GuardClauses;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class Person : BaseAuditableEntity
{
    private readonly HashSet<PersonAddress> _addresses = [];
    private readonly HashSet<PersonContact> _contacts = [];
    private readonly HashSet<PersonBankAccount> _bankAccounts = [];

    private Person()
    {
    }

    public PersonType Type { get; private set; }
    public string? FirstName { get; private set; }
    public string? MiddleName { get; private set; }
    public string? LastName { get; private set; }
    public string? CompanyName { get; private set; }
    public string? Egn { get; private set; }
    public string? Eik { get; private set; }
    public string VatNumber { get; private set; } = null!;
    public string SearchName { get; private set; } = null!;
    public string SearchTaxIdentifier { get; private set; } = null!;

    public IReadOnlyCollection<PersonAddress> Addresses => _addresses;
    public IReadOnlyCollection<PersonContact> Contacts => _contacts;
    public IReadOnlyCollection<PersonBankAccount> BankAccounts => _bankAccounts;

    [NotMapped]
    public string DisplayName => Type == PersonType.Company ? CompanyName! : BuildFullName();

    public static Person CreateIndividual(
        string firstName,
        string lastName,
        string egn,
        string vatNumber,
        string? middleName = null
    )
    {
        var person = new Person { Id = Guid.NewGuid() };
        person.UpdateIndividual(firstName, lastName, egn, vatNumber, middleName);
        return person;
    }

    public static Person CreateCompany(string companyName, string eik, string vatNumber)
    {
        var person = new Person { Id = Guid.NewGuid() };
        person.UpdateCompany(companyName, eik, vatNumber);
        return person;
    }

    public void UpdateIndividual(
        string firstName,
        string lastName,
        string egn,
        string vatNumber,
        string? middleName = null
    )
    {
        var normalizedFirstName = NormalizeRequiredName(firstName, nameof(firstName));
        var normalizedMiddleName = NormalizeOptionalName(middleName);
        var normalizedLastName = NormalizeRequiredName(lastName, nameof(lastName));
        var normalizedEgn = NormalizeTaxIdentifier(Guard.Against.NullOrWhiteSpace(egn, nameof(egn)));
        Guard.Against.OutOfRange(
            normalizedEgn.Length,
            nameof(egn),
            10,
            10,
            "EGN must contain exactly 10 digits."
        );

        Type = PersonType.Individual;
        FirstName = normalizedFirstName;
        MiddleName = normalizedMiddleName;
        LastName = normalizedLastName;
        CompanyName = null;
        Egn = normalizedEgn;
        Eik = null;

        RefreshDerivedValues(vatNumber);
    }

    public void UpdateCompany(string companyName, string eik, string vatNumber)
    {
        var normalizedCompanyName = NormalizeRequiredName(companyName, nameof(companyName));
        var normalizedEik = NormalizeTaxIdentifier(Guard.Against.NullOrWhiteSpace(eik, nameof(eik)));
        Guard.Against.OutOfRange(
            normalizedEik.Length,
            nameof(eik),
            9,
            13,
            "EIK must contain between 9 and 13 digits."
        );

        Type = PersonType.Company;
        CompanyName = normalizedCompanyName;
        FirstName = null;
        MiddleName = null;
        LastName = null;
        Egn = null;
        Eik = normalizedEik;

        RefreshDerivedValues(vatNumber);
    }

    public void AddAddress(PersonAddress address) => _addresses.Add(Guard.Against.Null(address));

    public void RemoveAddress(PersonAddress address) => _addresses.Remove(Guard.Against.Null(address));

    public void ClearAddresses() => _addresses.Clear();

    public void AddContact(PersonContact contact) => _contacts.Add(Guard.Against.Null(contact));

    public void RemoveContact(PersonContact contact) => _contacts.Remove(Guard.Against.Null(contact));

    public void ClearContacts() => _contacts.Clear();

    public void AddBankAccount(PersonBankAccount bankAccount) =>
        _bankAccounts.Add(Guard.Against.Null(bankAccount));

    public void RemoveBankAccount(PersonBankAccount bankAccount) =>
        _bankAccounts.Remove(Guard.Against.Null(bankAccount));

    public void ClearBankAccounts() => _bankAccounts.Clear();

    private void RefreshDerivedValues(string vatNumber)
    {
        SearchName = NormalizeSearchValue(BuildSearchName());
        SearchTaxIdentifier = NormalizeTaxIdentifier(Type == PersonType.Company ? Eik! : Egn!);
        VatNumber = NormalizeVatNumber(vatNumber);
    }

    private string BuildSearchName() => Type == PersonType.Company ? CompanyName! : BuildFullName();

    private string BuildFullName()
    {
        var nameParts = new[] { FirstName, MiddleName, LastName }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part!.Trim());

        return string.Join(" ", nameParts);
    }

    private static string NormalizeSearchValue(string value) =>
        string.Join(" ", value.Trim().ToUpperInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));

    private static string NormalizeTaxIdentifier(string value) =>
        new(value.Where(char.IsDigit).ToArray());

    private static string NormalizeVatNumber(string vatNumber)
    {
        var normalizedVatNumber = Guard.Against.NullOrWhiteSpace(vatNumber, nameof(vatNumber)).Trim();
        Guard.Against.OutOfRange(
            normalizedVatNumber.Length,
            nameof(vatNumber),
            1,
            20,
            "VAT number must contain at most 20 characters."
        );

        return normalizedVatNumber;
    }

    private static string NormalizeRequiredName(string value, string parameterName) =>
        Guard.Against.NullOrWhiteSpace(value, parameterName).Trim();

    private static string? NormalizeOptionalName(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
