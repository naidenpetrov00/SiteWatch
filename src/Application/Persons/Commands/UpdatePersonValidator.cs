using FluentValidation;

namespace Application.Persons.Commands;

public sealed class UpdatePersonValidator : AbstractValidator<UpdatePersonCommand>
{
    public UpdatePersonValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x)
            .Must(HasAnyUpdates)
            .WithMessage("At least one field must be provided.");

        When(x => x.FirstName is not null, () =>
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(100);
        });

        When(x => x.MiddleName is not null, () =>
        {
            RuleFor(x => x.MiddleName)
                .MaximumLength(100);
        });

        When(x => x.LastName is not null, () =>
        {
            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(100);
        });

        When(x => x.CompanyName is not null, () =>
        {
            RuleFor(x => x.CompanyName)
                .NotEmpty()
                .MaximumLength(250);
        });

        When(x => x.Addresses is not null, () =>
        {
            RuleFor(x => x.Addresses)
                .Must(PersonValidationRules.HaveAtMostOnePrimary)
                .WithMessage("Only one primary address is allowed.");
            RuleForEach(x => x.Addresses!).SetValidator(new PersonAddressDtoValidator());
        });

        When(x => x.Contacts is not null, () =>
        {
            RuleFor(x => x.Contacts)
                .Must(PersonValidationRules.HaveAtMostOnePrimary)
                .WithMessage("Only one primary contact is allowed.");
            RuleForEach(x => x.Contacts!).SetValidator(new PersonContactDtoValidator());
        });

        When(x => x.BankAccounts is not null, () =>
        {
            RuleFor(x => x.BankAccounts)
                .Must(PersonValidationRules.HaveAtMostOnePrimary)
                .WithMessage("Only one primary bank account is allowed.");
            RuleForEach(x => x.BankAccounts!).SetValidator(new PersonBankAccountDtoValidator());
        });
    }

    private static bool HasAnyUpdates(UpdatePersonCommand request)
    {
        var hasPrimaryInfo =
            request.FirstName is not null ||
            request.MiddleName is not null ||
            request.LastName is not null ||
            request.CompanyName is not null;

        var hasCollections =
            request.Addresses is not null ||
            request.Contacts is not null ||
            request.BankAccounts is not null;

        return hasPrimaryInfo || hasCollections;
    }
}
