using Domain.Entities;
using Domain.SeedWork.Enums;
using FluentValidation;

namespace Application.Assets;

public sealed class MobileAssetByQrValidator : AbstractValidator<MobileAssetByQrQuery>
{
    public MobileAssetByQrValidator() => RuleFor(x => x.QrIdentifier)
        .Must(MobileAssetValidation.IsCanonicalQrIdentifier)
        .WithMessage("QrIdentifier must be a canonical lowercase GUID.");
}

public sealed class MobileAssetsValidator : AbstractValidator<MobileAssetsQuery>
{
    public MobileAssetsValidator()
    {
        RuleFor(x => x.Search).Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Search is required.")
            .MaximumLength(200);
        RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 20);
    }
}

public sealed class MobileAssetByIdValidator : AbstractValidator<MobileAssetByIdQuery>
{
    public MobileAssetByIdValidator() => RuleFor(x => x.Id).NotEmpty();
}

public sealed class MobileAssetRegistrationByIdValidator
    : AbstractValidator<MobileAssetRegistrationByIdQuery>
{
    public MobileAssetRegistrationByIdValidator() => RuleFor(x => x.Id).NotEmpty();
}

public sealed class MobileAssetSitesValidator : AbstractValidator<MobileAssetSitesQuery>
{
    public MobileAssetSitesValidator() => RuleFor(x => x.Search).MaximumLength(100);
}

public sealed class RegisterMobileAssetValidator : AbstractValidator<RegisterMobileAssetCommand>
{
    public RegisterMobileAssetValidator()
    {
        RuleFor(x => x.Classification)
            .Must(AssetValidatorExtensions.IsEnum<AssetClassification>);
        RuleFor(x => x.Name)
            .Must(value => MobileAssetValidation.ValidRequiredCollapsed(
                value,
                Asset.MaxNameLength))
            .WithMessage($"Name is required and cannot exceed {Asset.MaxNameLength} characters after whitespace normalization.");
        RuleFor(x => x.Manufacturer)
            .Must(value => MobileAssetValidation.ValidOptionalCollapsed(
                value,
                Asset.MaxManufacturerLength))
            .WithMessage($"Manufacturer cannot exceed {Asset.MaxManufacturerLength} characters after whitespace normalization.");
        RuleFor(x => x.Model)
            .Must(value => MobileAssetValidation.ValidOptionalCollapsed(
                value,
                Asset.MaxModelLength))
            .WithMessage($"Model cannot exceed {Asset.MaxModelLength} characters after whitespace normalization.");
        RuleFor(x => x.Description)
            .Must(value => MobileAssetValidation.ValidOptionalTrimmed(
                value,
                Asset.MaxDescriptionLength))
            .WithMessage($"Description cannot exceed {Asset.MaxDescriptionLength} characters after trimming.");
        RuleFor(x => x.ManufacturerSerialNumber)
            .Must(value => MobileAssetValidation.ValidOptionalTrimmed(
                value,
                Asset.MaxManufacturerSerialNumberLength))
            .WithMessage($"ManufacturerSerialNumber cannot exceed {Asset.MaxManufacturerSerialNumberLength} characters after trimming.");
        RuleFor(x => x.SiteId).NotEmpty();
    }
}

internal static class MobileAssetValidation
{
    internal static bool IsCanonicalQrIdentifier(string? value) =>
        value is not null
        && Guid.TryParseExact(value, "D", out var parsed)
        && string.Equals(value, parsed.ToString("D"), StringComparison.Ordinal);

    internal static bool ValidRequiredCollapsed(string? value, int maxLength)
    {
        var normalized = CollapseWhitespace(value);
        return normalized.Length is > 0 && normalized.Length <= maxLength;
    }

    internal static bool ValidOptionalCollapsed(string? value, int maxLength) =>
        string.IsNullOrWhiteSpace(value) || CollapseWhitespace(value).Length <= maxLength;

    internal static bool ValidOptionalTrimmed(string? value, int maxLength) =>
        string.IsNullOrWhiteSpace(value) || value.Trim().Length <= maxLength;

    private static string CollapseWhitespace(string? value) =>
        string.Join(" ", (value ?? string.Empty).Split(
            (char[]?)null,
            StringSplitOptions.RemoveEmptyEntries));
}
