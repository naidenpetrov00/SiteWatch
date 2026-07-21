using FluentValidation;

namespace Application.Persons.Commands;

internal static class PersonValidationRules
{
    internal static bool HaveAtMostOnePrimary<T>(IEnumerable<T>? items)
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
