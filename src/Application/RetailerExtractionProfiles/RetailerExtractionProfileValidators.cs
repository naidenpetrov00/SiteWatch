using Domain.Entities;
using Domain.SeedWork.Enums;
using Domain.ValueObjects;
using FluentValidation;

namespace Application.RetailerExtractionProfiles;

public sealed class RetailerExtractionProfileVersionsValidator
    : AbstractValidator<RetailerExtractionProfileVersionsQuery>
{
    public RetailerExtractionProfileVersionsValidator() =>
        RuleFor(request => request.RetailerId).NotEmpty();
}

public sealed class RetailerExtractionCurrentProfilesValidator
    : AbstractValidator<RetailerExtractionCurrentProfilesQuery>
{
    public RetailerExtractionCurrentProfilesValidator() =>
        RuleFor(request => request.RetailerId).NotEmpty();
}

public sealed class RetailerExtractionProfileByIdValidator
    : AbstractValidator<RetailerExtractionProfileByIdQuery>
{
    public RetailerExtractionProfileByIdValidator()
    {
        RuleFor(request => request.RetailerId).NotEmpty();
        RuleFor(request => request.ProfileId).NotEmpty();
    }
}

public sealed class CreateRetailerExtractionDraftValidator
    : AbstractValidator<CreateRetailerExtractionDraftCommand>
{
    public CreateRetailerExtractionDraftValidator()
    {
        RuleFor(request => request.RetailerId).NotEmpty();
        RuleFor(request => request.SourcePublishedProfileId)
            .NotEqual(Guid.Empty)
            .When(request => request.SourcePublishedProfileId.HasValue);
    }
}

public sealed class DeleteRetailerExtractionDraftValidator
    : AbstractValidator<DeleteRetailerExtractionDraftCommand>
{
    public DeleteRetailerExtractionDraftValidator()
    {
        RuleFor(request => request.RetailerId).NotEmpty();
        RuleFor(request => request.ProfileId).NotEmpty();
    }
}

public sealed class UpdateRetailerExtractionAllowedHostsValidator
    : AbstractValidator<UpdateRetailerExtractionAllowedHostsCommand>
{
    public UpdateRetailerExtractionAllowedHostsValidator()
    {
        RuleFor(request => request.RetailerId).NotEmpty();
        RuleFor(request => request.ProfileId).NotEmpty();
        RuleFor(request => request.AllowedHosts).NotNull();
        RuleForEach(request => request.AllowedHosts)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(host => RetailerExtractionHost.TryCreate(host, out _))
            .WithMessage(
                "Allowed hosts must be exact DNS hostnames without a scheme, port, path, wildcard, or IP address.");
        RuleFor(request => request.AllowedHosts)
            .Must(hosts => hosts is not null && hosts
                .Select(host => RetailerExtractionHost.TryCreate(host, out var normalized)
                    ? normalized.Value
                    : host)
                .Distinct(StringComparer.Ordinal)
                .Count() == hosts.Count)
            .WithMessage("Allowed hosts must be unique after normalization.");
    }
}

public sealed class AddRetailerExtractionRuleValidator
    : RetailerExtractionRuleRequestValidator<AddRetailerExtractionRuleCommand>
{
    public AddRetailerExtractionRuleValidator()
    {
        RuleFor(request => request.RetailerId).NotEmpty();
        RuleFor(request => request.ProfileId).NotEmpty();
    }
}

public sealed class UpdateRetailerExtractionRuleValidator
    : RetailerExtractionRuleRequestValidator<UpdateRetailerExtractionRuleCommand>
{
    public UpdateRetailerExtractionRuleValidator()
    {
        RuleFor(request => request.RetailerId).NotEmpty();
        RuleFor(request => request.ProfileId).NotEmpty();
        RuleFor(request => request.RuleId).NotEmpty();
    }
}

public sealed class DeleteRetailerExtractionRuleValidator
    : AbstractValidator<DeleteRetailerExtractionRuleCommand>
{
    public DeleteRetailerExtractionRuleValidator()
    {
        RuleFor(request => request.RetailerId).NotEmpty();
        RuleFor(request => request.ProfileId).NotEmpty();
        RuleFor(request => request.RuleId).NotEmpty();
    }
}

public sealed class SetRetailerExtractionRuleEnabledValidator
    : AbstractValidator<SetRetailerExtractionRuleEnabledCommand>
{
    public SetRetailerExtractionRuleEnabledValidator()
    {
        RuleFor(request => request.RetailerId).NotEmpty();
        RuleFor(request => request.ProfileId).NotEmpty();
        RuleFor(request => request.RuleId).NotEmpty();
    }
}

public sealed class ReorderRetailerExtractionRulesValidator
    : AbstractValidator<ReorderRetailerExtractionRulesCommand>
{
    public ReorderRetailerExtractionRulesValidator()
    {
        RuleFor(request => request.RetailerId).NotEmpty();
        RuleFor(request => request.ProfileId).NotEmpty();
        RuleFor(request => request.OrderedRuleIds).NotNull();
        RuleForEach(request => request.OrderedRuleIds).NotEmpty();
        RuleFor(request => request.OrderedRuleIds)
            .Must(ids => ids is not null && ids.Distinct().Count() == ids.Count)
            .WithMessage("OrderedRuleIds cannot contain duplicates.");
    }
}

public sealed class PublishRetailerExtractionProfileValidator
    : AbstractValidator<PublishRetailerExtractionProfileCommand>
{
    public PublishRetailerExtractionProfileValidator()
    {
        RuleFor(request => request.RetailerId).NotEmpty();
        RuleFor(request => request.ProfileId).NotEmpty();
    }
}

public sealed class ActivateRetailerExtractionProfileValidator
    : AbstractValidator<ActivateRetailerExtractionProfileCommand>
{
    public ActivateRetailerExtractionProfileValidator()
    {
        RuleFor(request => request.RetailerId).NotEmpty();
        RuleFor(request => request.ProfileId).NotEmpty();
    }
}

public abstract class RetailerExtractionRuleRequestValidator<TRequest>
    : AbstractValidator<TRequest>
    where TRequest : RetailerExtractionRuleRequest
{
    protected RetailerExtractionRuleRequestValidator()
    {
        RuleFor(request => request.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(RetailerExtractionRule.MaxNameLength)
            .Must(HaveNoControlCharacters)
            .WithMessage("Name cannot contain control characters.")
            .Must(value => value.Any(char.IsLetterOrDigit))
            .WithMessage("Name must contain a letter or digit.");
        RuleFor(request => request.RuleType)
            .Must(value => RetailerExtractionCodes.TryParseRuleType(value, out _))
            .WithMessage("RuleType must be jsonLd or cssSelector.");

        When(
            request => IsRuleType(request.RuleType, RetailerExtractionRuleType.JsonLd),
            () =>
            {
                RuleFor(request => request.JsonLdObjectType)
                    .NotEmpty()
                    .MaximumLength(RetailerExtractionRule.MaxObjectTypeLength)
                    .Must(HaveNoControlCharacters);
                RuleFor(request => request.JsonLdPricePath)
                    .NotEmpty()
                    .MaximumLength(RetailerExtractionRule.MaxPathLength)
                    .Must(HaveNoControlCharacters);
                RuleFor(request => request.JsonLdCurrencyPath)
                    .MaximumLength(RetailerExtractionRule.MaxPathLength)
                    .Must(HaveNoControlCharacters);
                RuleFor(request => request.CssSelector).Empty();
                RuleFor(request => request.CssValueSource).Empty();
                RuleFor(request => request.CssAttributeName).Empty();
            });

        When(
            request => IsRuleType(request.RuleType, RetailerExtractionRuleType.CssSelector),
            () =>
            {
                RuleFor(request => request.CssSelector)
                    .NotEmpty()
                    .MaximumLength(RetailerExtractionRule.MaxSelectorLength)
                    .Must(HaveNoControlCharacters);
                RuleFor(request => request.CssValueSource)
                    .Must(value => RetailerExtractionCodes.TryParseCssValueSource(value, out _))
                    .WithMessage("CssValueSource must be textContent or attribute.");
                RuleFor(request => request.CssAttributeName)
                    .NotEmpty()
                    .MaximumLength(RetailerExtractionRule.MaxAttributeNameLength)
                    .Must(HaveNoControlCharacters)
                    .When(request => IsCssValueSource(
                        request.CssValueSource,
                        RetailerExtractionCssValueSource.Attribute));
                RuleFor(request => request.CssAttributeName)
                    .Empty()
                    .When(request => IsCssValueSource(
                        request.CssValueSource,
                        RetailerExtractionCssValueSource.TextContent));
                RuleFor(request => request.JsonLdObjectType).Empty();
                RuleFor(request => request.JsonLdPricePath).Empty();
                RuleFor(request => request.JsonLdCurrencyPath).Empty();
            });

        RuleFor(request => request.DecimalSeparator)
            .Must(value => RetailerExtractionCodes.TryParseDecimalSeparator(value, out _))
            .WithMessage("DecimalSeparator must be dot or comma.");
        RuleFor(request => request.ThousandsSeparator)
            .Must(value => RetailerExtractionCodes.TryParseThousandsSeparator(value, out _))
            .WithMessage("ThousandsSeparator must be none, dot, comma, or space.");
        RuleFor(request => request)
            .Must(HaveDistinctSeparators)
            .WithName(nameof(RetailerExtractionRuleRequest.ThousandsSeparator))
            .WithMessage("Decimal and thousands separators must differ.");
        RuleFor(request => request.ExpectedCurrencyCode)
            .Equal(RetailerExtractionRule.EuroCurrencyCode)
            .WithMessage("ExpectedCurrencyCode must be EUR.");
        RuleFor(request => request.PriceBasis)
            .Must(value => PriceBasisCodes.TryParse(value, out _))
            .WithMessage("PriceBasis must be item or package.");
        RuleFor(request => request.MinimumValue)
            .Must(BeValidOptionalAmount)
            .WithMessage("MinimumValue must be positive and have at most two decimal places.");
        RuleFor(request => request.MaximumValue)
            .Must(BeValidOptionalAmount)
            .WithMessage("MaximumValue must be positive and have at most two decimal places.");
        RuleFor(request => request)
            .Must(request => !request.MinimumValue.HasValue
                || !request.MaximumValue.HasValue
                || request.MinimumValue.Value <= request.MaximumValue.Value)
            .WithName(nameof(RetailerExtractionRuleRequest.MaximumValue))
            .WithMessage("MaximumValue must be greater than or equal to MinimumValue.");
    }

    private static bool HaveNoControlCharacters(string? value) =>
        string.IsNullOrEmpty(value) || !value.Any(char.IsControl);

    private static bool IsRuleType(string? value, RetailerExtractionRuleType expected) =>
        RetailerExtractionCodes.TryParseRuleType(value, out var actual) && actual == expected;

    private static bool IsCssValueSource(
        string? value,
        RetailerExtractionCssValueSource expected) =>
        RetailerExtractionCodes.TryParseCssValueSource(value, out var actual)
        && actual == expected;

    private static bool HaveDistinctSeparators(TRequest request)
    {
        if (!RetailerExtractionCodes.TryParseDecimalSeparator(
                request.DecimalSeparator,
                out var decimalSeparator)
            || !RetailerExtractionCodes.TryParseThousandsSeparator(
                request.ThousandsSeparator,
                out var thousandsSeparator))
        {
            return true;
        }

        return (thousandsSeparator is RetailerExtractionThousandsSeparator.None
                or RetailerExtractionThousandsSeparator.Space)
            || (decimalSeparator == RetailerExtractionDecimalSeparator.Dot
                ? thousandsSeparator != RetailerExtractionThousandsSeparator.Dot
                : thousandsSeparator != RetailerExtractionThousandsSeparator.Comma);
    }

    private static bool BeValidOptionalAmount(decimal? value) =>
        !value.HasValue
        || (value.Value > 0m
            && value.Value <= RetailerExtractionRule.MaximumAcceptedValue
            && decimal.Round(value.Value, 2) == value.Value);
}
