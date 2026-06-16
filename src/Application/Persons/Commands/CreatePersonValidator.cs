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
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.MiddleName).MaximumLength(100);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Egn).NotEmpty().Must(HaveExactlyTenDigits).WithMessage("EGN must contain exactly 10 digits.");
            RuleFor(x => x.CompanyName).Must(string.IsNullOrWhiteSpace);
            RuleFor(x => x.Eik).Must(string.IsNullOrWhiteSpace);
        });

        When(x => x.Type == PersonType.Company, () =>
        {
            RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(250);
            RuleFor(x => x.Eik).NotEmpty().Must(HaveBetweenNineAndThirteenDigits).WithMessage("EIK must contain between 9 and 13 digits.");
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

    private static bool HaveExactlyTenDigits(string? value) => NormalizeDigits(value).Length == 10;

    private static bool HaveBetweenNineAndThirteenDigits(string? value)
    {
        var digits = NormalizeDigits(value);
        return digits.Length is >= 9 and <= 13;
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

    protected static string NormalizeDigits(string? value) =>
        new(value?.Where(char.IsDigit).ToArray() ?? []);
}

internal sealed class PersonAddressDtoValidator : AbstractValidator<PersonAddressDto>
{
    public PersonAddressDtoValidator()
    {
        RuleFor(x => x.AddressLine).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AdditionalLine).MaximumLength(200);
        RuleFor(x => x.City).MaximumLength(100);
        RuleFor(x => x.PostalCode).MaximumLength(20);
        RuleFor(x => x.Country).MaximumLength(100);
        RuleFor(x => x.Details).MaximumLength(500);
    }
}

internal sealed class PersonContactDtoValidator : AbstractValidator<PersonContactDto>
{
    public PersonContactDtoValidator()
    {
        RuleFor(x => x.ContactType).IsInEnum();
        RuleFor(x => x.Value).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Details).MaximumLength(500);
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
