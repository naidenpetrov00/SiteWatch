using System.Data.Common;
using Application.Persons;
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
        await ValidateTerritorialAddressesAsync(request.Addresses, cancellationToken);
        var person = CreatePerson(request);
        await EnsureIdentifierIsUniqueAsync(person, cancellationToken);
        ApplyChildCollections(person, request);

        dbContext.Persons.Add(person);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            throw new PersonConflictException(GetDuplicateIdentifierMessage(person), exception);
        }

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
            await ValidateTerritorialAddressesAsync(request.Addresses, cancellationToken);
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
        if (await dbContext.RetailerExtractionProfiles.AnyAsync(
                profile => profile.CompanyPersonId == id,
                cancellationToken))
        {
            throw new PersonConflictException(
                "This company Person owns retailer extraction profiles and cannot be deleted.");
        }

        if (await dbContext.RetailerPriceCollectionRuns.AnyAsync(
                run => run.CompanyPersonId == id,
                cancellationToken))
        {
            throw new PersonConflictException(
                "This company Person owns retained price-collection runs and cannot be deleted.");
        }

        if (await dbContext.Retailers.AnyAsync(
                retailer => retailer.CompanyPersonId == id,
                cancellationToken))
        {
            throw new PersonConflictException(
                "This company Person is referenced by one or more retailers and cannot be deleted.");
        }

        int deletedRows;
        try
        {
            deletedRows = await dbContext.Persons
                .Where(x => x.Id == id)
                .ExecuteDeleteAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            throw new PersonConflictException(
                "This Person is referenced by retained business data and cannot be deleted.",
                exception);
        }
        catch (DbException exception)
        {
            throw new PersonConflictException(
                "This Person is referenced by retained business data and cannot be deleted.",
                exception);
        }

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

    private async Task EnsureIdentifierIsUniqueAsync(Person person, CancellationToken cancellationToken)
    {
        var duplicateExists = person.Type == PersonType.Company
            ? await dbContext.Persons.AsNoTracking().AnyAsync(
                existing => existing.Eik == person.Eik,
                cancellationToken)
            : await dbContext.Persons.AsNoTracking().AnyAsync(
                existing => existing.Egn == person.Egn,
                cancellationToken);

        if (duplicateExists)
        {
            throw new PersonConflictException(GetDuplicateIdentifierMessage(person));
        }
    }

    private static string GetDuplicateIdentifierMessage(Person person) =>
        person.Type == PersonType.Company
            ? "A person with this EIK already exists."
            : "A person with this EGN already exists.";

    private static CompanyLegalForm? ParseLegalForm(string? legalForm)
    {
        var value = Guard.Against.NullOrWhiteSpace(legalForm, nameof(legalForm)).Trim();

        if (!Enum.TryParse<CompanyLegalForm>(value, true, out var parsed))
        {
            throw new ArgumentException($"Unsupported legal form '{value}'.", nameof(legalForm));
        }

        return parsed;
    }

    private async Task ValidateTerritorialAddressesAsync(
        IEnumerable<PersonAddressDto>? addresses,
        CancellationToken cancellationToken)
    {
        if (addresses is null) return;

        foreach (var address in addresses)
        {
            if (address.MunicipalityId.HasValue && !await dbContext.Municipalities.AnyAsync(
                    x => x.Id == address.MunicipalityId.Value, cancellationToken))
                throw new PersonConflictException("The selected Municipality does not exist.");

            if (!address.SettlementId.HasValue && (address.DistrictId.HasValue || address.QuarterId.HasValue))
                throw new PersonConflictException("A District or Quarter requires a Settlement.");

            if (address.SettlementId.HasValue)
            {
                var settlement = await dbContext.Settlements.AsNoTracking().SingleOrDefaultAsync(
                    x => x.Id == address.SettlementId.Value, cancellationToken);
                if (settlement is null || !address.MunicipalityId.HasValue || settlement.MunicipalityId != address.MunicipalityId.Value)
                    throw new PersonConflictException("The selected Settlement does not belong to the selected Municipality.");
            }

            if (address.DistrictId.HasValue && !await dbContext.Districts.AnyAsync(
                    x => x.Id == address.DistrictId.Value && x.SettlementId == address.SettlementId, cancellationToken))
                throw new PersonConflictException("The selected District does not belong to the selected Settlement.");

            if (address.QuarterId.HasValue && !await dbContext.Quarters.AnyAsync(
                    x => x.Id == address.QuarterId.Value && x.SettlementId == address.SettlementId, cancellationToken))
                throw new PersonConflictException("The selected Quarter does not belong to the selected Settlement.");
        }
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
                    address.Country,
                    address.MunicipalityId,
                    address.SettlementId,
                    address.DistrictId,
                    address.QuarterId,
                    address.PostalCode,
                    address.Street,
                    address.Number,
                    address.Building,
                    address.Entrance,
                    address.Floor,
                    address.Apartment,
                    address.AddressDetails,
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
                    address.Country,
                    address.MunicipalityId,
                    address.SettlementId,
                    address.DistrictId,
                    address.QuarterId,
                    address.PostalCode,
                    address.Street,
                    address.Number,
                    address.Building,
                    address.Entrance,
                    address.Floor,
                    address.Apartment,
                    address.AddressDetails,
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
