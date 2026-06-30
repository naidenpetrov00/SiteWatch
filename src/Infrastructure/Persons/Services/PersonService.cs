using Application.Persons.Commands;
using Application.SeedWork.Interfaces;
using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.SeedWork.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persons.Services;

public sealed class PersonService(ApplicationDbContext dbContext) : IPersonService
{
    public async Task<Guid> CreateAsync(PersonUpsertDto request, CancellationToken cancellationToken)
    {
        var person = CreatePerson(request);
        ApplyChildCollections(person, request);

        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync(cancellationToken);

        return person.Id;
    }

    public async Task UpdateAsync(UpdatePersonCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var person = await dbContext.Persons
            .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (person is null)
        {
            throw new NotFoundException(nameof(Person), request.Id.ToString());
        }

        if (person.Type == PersonType.Individual)
        {
            if (request.FirstName is not null)
            {
                person.UpdateFirstName(request.FirstName);
            }

            if (request.MiddleName is not null)
            {
                person.UpdateMiddleName(request.MiddleName);
            }

            if (request.LastName is not null)
            {
                person.UpdateLastName(request.LastName);
            }
        }
        else if (request.CompanyName is not null)
        {
            person.UpdateCompanyName(request.CompanyName);
        }

        if (request.Addresses is not null)
        {
            await ReplaceAddressesAsync(person.Id, request.Addresses, cancellationToken);
        }

        if (request.Contacts is not null)
        {
            await ReplaceContactsAsync(person.Id, request.Contacts, cancellationToken);
        }

        if (request.BankAccounts is not null)
        {
            await ReplaceBankAccountsAsync(person.Id, request.BankAccounts, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deletedRows = await dbContext.Persons
            .Where(x => x.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        if (deletedRows == 0)
        {
            throw new NotFoundException(nameof(Person), id.ToString());
        }
    }

    private static Person CreatePerson(PersonUpsertDto request) => request.Type switch
    {
        PersonType.Company => Person.CreateCompany(
            request.CompanyName!,
            ParseLegalForm(request.LegalForm),
            request.Eik!,
            request.VatNumber!
        ),
        PersonType.Individual => Person.CreateIndividual(
            request.FirstName!,
            request.LastName!,
            request.Egn!,
            request.VatNumber!,
            request.MiddleName
        ),
        _ => throw new InvalidOperationException("Person type is required.")
    };

    private static CompanyLegalForm? ParseLegalForm(string? legalForm)
    {
        var value = Guard.Against.NullOrWhiteSpace(legalForm, nameof(legalForm)).Trim();

        if (!Enum.TryParse<CompanyLegalForm>(value, true, out var parsed))
        {
            throw new ArgumentException($"Unsupported legal form '{value}'.", nameof(legalForm));
        }

        return parsed;
    }

    private static void ApplyChildCollections(Person person, PersonUpsertDto request)
    {
        AddAddresses(person, request);
        AddContacts(person, request);
        AddBankAccounts(person, request);
    }

    private static void AddAddresses(Person person, PersonUpsertDto request)
    {
        if (request.Addresses is null)
        {
            return;
        }

        foreach (var address in request.Addresses)
        {
            person.AddAddress(
                PersonAddress.Create(
                    person.Id,
                    address.AddressLine,
                    address.City,
                    address.PostalCode,
                    address.Country,
                    address.AdditionalLine,
                    address.Details,
                    address.IsPrimary,
                    address.IsActive
                )
            );
        }
    }

    private static void AddContacts(Person person, PersonUpsertDto request)
    {
        if (request.Contacts is null)
        {
            return;
        }

        foreach (var contact in request.Contacts)
        {
            person.AddContact(
                PersonContact.Create(
                    person.Id,
                    contact.ContactType,
                    contact.Value,
                    contact.Details,
                    contact.IsPrimary,
                    contact.IsActive
                )
            );
        }
    }

    private static void AddBankAccounts(Person person, PersonUpsertDto request)
    {
        if (request.BankAccounts is null)
        {
            return;
        }

        foreach (var bankAccount in request.BankAccounts)
        {
            person.AddBankAccount(
                PersonBankAccount.Create(
                    person.Id,
                    bankAccount.IBAN,
                    bankAccount.BIC,
                    bankAccount.BankName,
                    bankAccount.Details,
                    bankAccount.IsPrimary,
                    bankAccount.IsActive
                )
            );
        }
    }

    private async Task ReplaceAddressesAsync(Guid personId, IReadOnlyCollection<PersonAddressDto> addresses, CancellationToken cancellationToken)
    {
        await dbContext.Set<PersonAddress>()
            .Where(x => x.PersonId == personId)
            .ExecuteDeleteAsync(cancellationToken);

        dbContext.Set<PersonAddress>().AddRange(
            addresses.Select(address =>
                PersonAddress.Create(
                    personId,
                    address.AddressLine,
                    address.City,
                    address.PostalCode,
                    address.Country,
                    address.AdditionalLine,
                    address.Details,
                    address.IsPrimary,
                    address.IsActive
                )
            )
        );
    }

    private async Task ReplaceContactsAsync(Guid personId, IReadOnlyCollection<PersonContactDto> contacts, CancellationToken cancellationToken)
    {
        await dbContext.Set<PersonContact>()
            .Where(x => x.PersonId == personId)
            .ExecuteDeleteAsync(cancellationToken);

        dbContext.Set<PersonContact>().AddRange(
            contacts.Select(contact =>
                PersonContact.Create(
                    personId,
                    contact.ContactType,
                    contact.Value,
                    contact.Details,
                    contact.IsPrimary,
                    contact.IsActive
                )
            )
        );
    }

    private async Task ReplaceBankAccountsAsync(
        Guid personId,
        IReadOnlyCollection<PersonBankAccountDto> bankAccounts,
        CancellationToken cancellationToken
    )
    {
        await dbContext.Set<PersonBankAccount>()
            .Where(x => x.PersonId == personId)
            .ExecuteDeleteAsync(cancellationToken);

        dbContext.Set<PersonBankAccount>().AddRange(
            bankAccounts.Select(bankAccount =>
                PersonBankAccount.Create(
                    personId,
                    bankAccount.IBAN,
                    bankAccount.BIC,
                    bankAccount.BankName,
                    bankAccount.Details,
                    bankAccount.IsPrimary,
                    bankAccount.IsActive
                )
            )
        );
    }

}
