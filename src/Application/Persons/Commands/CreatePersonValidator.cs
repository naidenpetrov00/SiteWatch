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
            .Must(PersonValidationRules.HaveAtMostOnePrimary)
            .When(x => x.Addresses is not null)
            .WithMessage("Only one primary address is allowed.");
        RuleFor(x => x.Contacts)
            .Must(PersonValidationRules.HaveAtMostOnePrimary)
            .When(x => x.Contacts is not null)
            .WithMessage("Only one primary contact is allowed.");
        RuleFor(x => x.BankAccounts)
            .Must(PersonValidationRules.HaveAtMostOnePrimary)
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
}
