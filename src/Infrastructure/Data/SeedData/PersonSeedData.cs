using Domain.Entities;
using Domain.SeedWork;
using Domain.SeedWork.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.SeedData;

public sealed class PersonSeedData(
    ApplicationDbContext dbContext,
    ILogger<PersonSeedData> logger)
{
    public const string PraktikerEik = "0000000000001";
    public const string MaxxmartEik = "0000000000002";

    private const string SeededBy = "System";

    public async Task<List<Person>> SeedAsync()
    {
        var definitions = CreateDefinitions();
        var persons = new List<Person>(definitions.Count);
        var addedCount = 0;

        foreach (var definition in definitions)
        {
            var person = await FindExistingAsync(definition);
            if (person is null)
            {
                person = definition.Create();
                ApplyAuditMetadata(person);
                AddAddress(person, definition.Address);
                AddBankAccount(person, definition.Iban);
                dbContext.Persons.Add(person);
                addedCount++;
            }

            persons.Add(person);
        }

        if (addedCount > 0)
        {
            await dbContext.SaveChangesAsync();
        }

        logger.LogInformation(
            "Ensured {PersonCount} seeded persons; created {CreatedPersonCount}.",
            persons.Count,
            addedCount);

        return persons;
    }

    private Task<Person?> FindExistingAsync(PersonDefinition definition) =>
        definition.Type == PersonType.Individual
            ? dbContext.Persons.SingleOrDefaultAsync(person =>
                person.Type == PersonType.Individual
                && person.Egn == definition.TaxIdentifier)
            : dbContext.Persons.SingleOrDefaultAsync(person =>
                person.Type == PersonType.Company
                && person.Eik == definition.TaxIdentifier);

    private static List<PersonDefinition> CreateDefinitions() =>
    [
        new(
            PersonType.Individual,
            "8001011234",
            () => Person.CreateIndividual("Ivan", "Petrov", "8001011234", "BG8001011234"),
            new AddressDefinition("Vitosha Boulevard 1", "Sofia", "1000"),
            "BG80BNBG96611020345678"),
        new(
            PersonType.Individual,
            "8502022345",
            () => Person.CreateIndividual("Maria", "Georgieva", "8502022345", "BG8502022345"),
            new AddressDefinition("Tsarigradsko Shose 115", "Sofia", "1784"),
            "BG18RZBB91550123456789"),
        new(
            PersonType.Company,
            "123456789",
            () => Person.CreateCompany("SiteWatch Services", null, "123456789", "BG123456789"),
            new AddressDefinition("Dondukov 11", "Sofia", "1000"),
            "BG03UNCR70001512345678"),
        new(
            PersonType.Company,
            PraktikerEik,
            () => Person.CreateCompany("Praktiker", null, PraktikerEik, "DEV-PRAKTIKER")),
        new(
            PersonType.Company,
            MaxxmartEik,
            () => Person.CreateCompany("Maxxmart", null, MaxxmartEik, "DEV-MAXXMART")),
    ];

    private static void AddAddress(Person person, AddressDefinition? definition)
    {
        if (definition is null)
        {
            return;
        }

        var address = PersonAddress.Create(
            person.Id,
            definition.AddressLine,
            definition.City,
            definition.PostalCode,
            "Bulgaria",
            isPrimary: true,
            isActive: true);
        ApplyAuditMetadata(address);
        person.AddAddress(address);
    }

    private static void AddBankAccount(Person person, string? iban)
    {
        if (iban is null)
        {
            return;
        }

        var bankAccount = PersonBankAccount.Create(
            person.Id,
            iban,
            isPrimary: true,
            isActive: true);
        ApplyAuditMetadata(bankAccount);
        person.AddBankAccount(bankAccount);
    }

    private static void ApplyAuditMetadata(BaseAuditableEntity entity)
    {
        var now = DateTimeOffset.UtcNow;
        entity.Created = now;
        entity.CreatedBy = SeededBy;
        entity.LastModified = now;
        entity.LastModifiedBy = SeededBy;
    }

    private sealed record PersonDefinition(
        PersonType Type,
        string TaxIdentifier,
        Func<Person> Create,
        AddressDefinition? Address = null,
        string? Iban = null);

    private sealed record AddressDefinition(
        string AddressLine,
        string City,
        string PostalCode);
}
