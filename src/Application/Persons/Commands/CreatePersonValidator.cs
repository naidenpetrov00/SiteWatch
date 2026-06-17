using System.Text.RegularExpressions;
using Domain.SeedWork.Enums;
using FluentValidation;

namespace Application.Persons.Commands;

public sealed class CreatePersonValidator : PersonUpsertValidator<CreatePersonCommand>
{
    public CreatePersonValidator()
    {
    }
}

public abstract class PersonUpsertValidator<TRequest> : AbstractValidator<TRequest>
    where TRequest : PersonUpsertDto
{
    private static readonly Regex LettersOnlyRegex = new(@"^[\p{L}\p{M}]+$", RegexOptions.CultureInvariant);
    private static readonly Regex CompanyNameRegex = new(@"^[\p{L}\p{M}\d ]+$", RegexOptions.CultureInvariant);
    private static readonly Regex DigitsOnlyRegex = new(@"^\d+$", RegexOptions.CultureInvariant);

    protected PersonUpsertValidator()
    {
        RuleFor(x => x.Type).NotNull().IsInEnum();
        RuleFor(x => x.VatNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Addresses)
            .Must(HaveAtMostOnePrimary)
            .When(x => x.Addresses is not null)
            .WithMessage("Only one primary address is allowed.");
        RuleFor(x => x.Contacts)
            .Must(HaveAtMostOnePrimary)
            .When(x => x.Contacts is not null)
            .WithMessage("Only one primary contact is allowed.");
        RuleFor(x => x.BankAccounts)
            .Must(HaveAtMostOnePrimary)
            .When(x => x.BankAccounts is not null)
            .WithMessage("Only one primary bank account is allowed.");

        When(x => x.Type == PersonType.Individual, () =>
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(100)
                .Matches(LettersOnlyRegex)
                .WithMessage("First name must contain letters only.");
            When(x => !string.IsNullOrWhiteSpace(x.MiddleName), () =>
            {
                RuleFor(x => x.MiddleName)
                    .MaximumLength(100)
                    .Matches(LettersOnlyRegex)
                    .WithMessage("Middle name must contain letters only.");
            });
            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(100)
                .Matches(LettersOnlyRegex)
                .WithMessage("Last name must contain letters only.");
            RuleFor(x => x.Egn)
                .NotEmpty()
                .Length(10)
                .Matches(DigitsOnlyRegex)
                .WithMessage("EGN must contain exactly 10 digits.");
            RuleFor(x => x.CompanyName).Must(string.IsNullOrWhiteSpace);
            RuleFor(x => x.LegalForm).Must(string.IsNullOrWhiteSpace);
            RuleFor(x => x.Eik).Must(string.IsNullOrWhiteSpace);
        });

        When(x => x.Type == PersonType.Company, () =>
        {
            RuleFor(x => x.CompanyName)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("Company name is required.")
                .MaximumLength(250)
                .Matches(CompanyNameRegex)
                .WithMessage("Company name must contain letters, digits, and spaces only.");
            RuleFor(x => x.LegalForm)
                .NotEmpty()
                .Must(value => Enum.TryParse<CompanyLegalForm>(value, true, out _))
                .WithMessage("Legal form must be one of the supported Bulgarian legal forms.");
            RuleFor(x => x.Eik)
                .NotEmpty()
                .Length(9, 13)
                .Matches(DigitsOnlyRegex)
                .WithMessage("EIK must contain between 9 and 13 digits.");
            RuleFor(x => x.FirstName).Must(string.IsNullOrWhiteSpace);
            RuleFor(x => x.MiddleName).Must(string.IsNullOrWhiteSpace);
            RuleFor(x => x.LastName).Must(string.IsNullOrWhiteSpace);
            RuleFor(x => x.Egn).Must(string.IsNullOrWhiteSpace);
        });

        When(x => x.Addresses is not null, () =>
        {
            RuleForEach(x => x.Addresses!).SetValidator(new PersonAddressDtoValidator());
        });

        When(x => x.Contacts is not null, () =>
        {
            RuleForEach(x => x.Contacts!).SetValidator(new PersonContactDtoValidator());
        });

        When(x => x.BankAccounts is not null, () =>
        {
            RuleForEach(x => x.BankAccounts!).SetValidator(new PersonBankAccountDtoValidator());
        });
    }

    private static bool HaveAtMostOnePrimary<T>(IEnumerable<T>? items)
        where T : class
    {
        if (items is null)
        {
            return true;
        }

        var primaryCount = items.Count(item => item is IPrimaryState state && state.IsPrimary);
        return primaryCount <= 1;
    }
}

internal sealed class PersonAddressDtoValidator : AbstractValidator<PersonAddressDto>
{
    private static readonly Regex DigitsOnlyRegex = new(@"^\d+$", RegexOptions.CultureInvariant);

    public PersonAddressDtoValidator()
    {
        RuleFor(x => x.AddressLine).MaximumLength(200);
        RuleFor(x => x.AdditionalLine).MaximumLength(200);
        RuleFor(x => x.City).MaximumLength(100);
        RuleFor(x => x.PostalCode)
            .MaximumLength(20)
            .Matches(DigitsOnlyRegex)
            .When(x => !string.IsNullOrWhiteSpace(x.PostalCode))
            .WithMessage("Postal code must contain only digits.");
        RuleFor(x => x.Country).MaximumLength(100);
        RuleFor(x => x.Details).MaximumLength(500);
    }
}

internal sealed class PersonContactDtoValidator : AbstractValidator<PersonContactDto>
{
    private static readonly Regex DigitsOnlyRegex = new(@"^\d+$", RegexOptions.CultureInvariant);

    public PersonContactDtoValidator()
    {
        RuleFor(x => x.ContactType).IsInEnum();
        RuleFor(x => x.Value).NotEmpty().MaximumLength(256);
        When(x => x.ContactType == Domain.SeedWork.Enums.ContactType.Phone, () =>
        {
            RuleFor(x => x.Value)
                .Length(8, 15)
                .Matches(DigitsOnlyRegex)
                .WithMessage("Phone number must contain between 8 and 15 digits.");
        });
        When(x => x.ContactType == Domain.SeedWork.Enums.ContactType.Email, () =>
        {
            RuleFor(x => x.Value)
                .EmailAddress()
                .WithMessage("Email must be a valid email address.");
        });
        When(x => x.ContactType == Domain.SeedWork.Enums.ContactType.Website, () =>
        {
            RuleFor(x => x.Value)
                .Must(HaveValidWebsite)
                .WithMessage("Website must be a valid website address.");
        });
        RuleFor(x => x.Details).MaximumLength(500);
    }

    private static bool HaveValidWebsite(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var trimmedValue = value.Trim();
        var candidate = trimmedValue.Contains("://", StringComparison.Ordinal) ? trimmedValue : $"https://{trimmedValue}";

        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || uri.Host.Contains('.');
    }
}

internal sealed class PersonBankAccountDtoValidator : AbstractValidator<PersonBankAccountDto>
{
    public PersonBankAccountDtoValidator()
    {
        RuleFor(x => x.IBAN).NotEmpty().MaximumLength(34);
        RuleFor(x => x.BIC).MaximumLength(11);
        RuleFor(x => x.BankName).MaximumLength(200);
        RuleFor(x => x.Details).MaximumLength(500);
    }
}

internal interface IPrimaryState
{
    bool IsPrimary { get; }
}
