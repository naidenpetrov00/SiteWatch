using Domain.Entities;
using Domain.SeedWork.Enums;
using FluentValidation;

namespace Application.Assets;

public abstract class AssetDetailsValidator<T> : AbstractValidator<T>
    where T : AssetDetailsRequest
{
    protected AssetDetailsValidator()
    {
        RuleFor(x => x.Classification).Must(AssetValidatorExtensions.IsEnum<AssetClassification>);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Asset.MaxNameLength);
        RuleFor(x => x.Manufacturer).MaximumLength(Asset.MaxManufacturerLength);
        RuleFor(x => x.Model).MaximumLength(Asset.MaxModelLength);
        RuleFor(x => x.Description).MaximumLength(Asset.MaxDescriptionLength);
    }
}

public sealed class RegisterAssetsValidator : AssetDetailsValidator<RegisterAssetsCommand>
{
    public RegisterAssetsValidator()
    {
        RuleFor(x => x.SiteId).NotEmpty();
        RuleFor(x => x.ResponsibleWorkerId).MaximumLength(450);
        RuleFor(x => x.Condition).Must(AssetValidatorExtensions.IsEnum<AssetCondition>);
        RuleFor(x => x.OperationalStatus)
            .Must(value => Enum.TryParse<AssetOperationalStatus>(value, true, out var status)
                && Enum.IsDefined(status)
                && status is not AssetOperationalStatus.Missing
                    and not AssetOperationalStatus.Retired
                    and not AssetOperationalStatus.InTransfer)
            .WithMessage("OperationalStatus must be Available, InUse, or UnderRepair.");
        RuleFor(x => x.Note).MaximumLength(AssetEvent.MaxNoteLength);
        RuleFor(x => x.Rows)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(x => x.Count is >= 1 and <= 50)
            .WithMessage("Rows must contain between 1 and 50 registrations.");
        RuleForEach(x => x.Rows).ChildRules(row => row
            .RuleFor(x => x.ManufacturerSerialNumber)
            .MaximumLength(Asset.MaxManufacturerSerialNumberLength));
    }
}

public abstract class AssetMutationValidator<T> : AbstractValidator<T>
    where T : AssetMutationRequest
{
    protected AssetMutationValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ConcurrencyToken).ValidConcurrencyToken();
        RuleFor(x => x.Note).MaximumLength(AssetEvent.MaxNoteLength);
    }
}

public sealed class ReviewAssetRegistrationValidator
    : AssetMutationValidator<ReviewAssetRegistrationCommand>;

public sealed class UpdateAssetDetailsValidator
    : AssetMutationValidator<UpdateAssetDetailsCommand>
{
    public UpdateAssetDetailsValidator()
    {
        RuleFor(x => x.Classification).Must(AssetValidatorExtensions.IsEnum<AssetClassification>);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Asset.MaxNameLength);
        RuleFor(x => x.Manufacturer).MaximumLength(Asset.MaxManufacturerLength);
        RuleFor(x => x.Model).MaximumLength(Asset.MaxModelLength);
        RuleFor(x => x.Description).MaximumLength(Asset.MaxDescriptionLength);
        RuleFor(x => x.ManufacturerSerialNumber)
            .MaximumLength(Asset.MaxManufacturerSerialNumberLength);
    }
}

public sealed class RelocateAssetValidator : AssetMutationValidator<RelocateAssetCommand>
{
    public RelocateAssetValidator() => RuleFor(x => x.SiteId).NotEmpty();
}

public sealed class ChangeAssetConditionValidator
    : AssetMutationValidator<ChangeAssetConditionCommand>
{
    public ChangeAssetConditionValidator() =>
        RuleFor(x => x.Condition).Must(AssetValidatorExtensions.IsEnum<AssetCondition>);
}

public sealed class ChangeAssetOperationalStatusValidator
    : AssetMutationValidator<ChangeAssetOperationalStatusCommand>
{
    public ChangeAssetOperationalStatusValidator() =>
        RuleFor(x => x.OperationalStatus)
            .Must(value => Enum.TryParse<AssetOperationalStatus>(value, true, out var status)
                && Enum.IsDefined(status)
                && status is not AssetOperationalStatus.Missing
                    and not AssetOperationalStatus.Retired
                    and not AssetOperationalStatus.InTransfer)
            .WithMessage("OperationalStatus must be Available, InUse, or UnderRepair.");
}

public sealed class ReportAssetMissingValidator
    : AssetMutationValidator<ReportAssetMissingCommand>;

public sealed class RetireAssetValidator : AssetMutationValidator<RetireAssetCommand>;

public sealed class RotateAssetQrIdentifierValidator
    : AssetMutationValidator<RotateAssetQrIdentifierCommand>;

public sealed class ReassignAssetResponsibilityValidator
    : AssetMutationValidator<ReassignAssetResponsibilityCommand>
{
    public ReassignAssetResponsibilityValidator()
    {
        RuleFor(x => x.ResponsibleWorkerId).MaximumLength(450);
        RuleFor(x => x.Reason)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Reason is required.")
            .MaximumLength(AssetEvent.MaxNoteLength);
    }
}

internal static class AssetValidatorExtensions
{
    internal static bool IsEnum<TEnum>(string value) where TEnum : struct, Enum =>
        Enum.TryParse<TEnum>(value, true, out var result) && Enum.IsDefined(result);

    internal static IRuleBuilderOptions<T, string> ValidConcurrencyToken<T>(
        this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .Must(value =>
            {
                try
                {
                    return Convert.FromBase64String(value).Length > 0;
                }
                catch (FormatException)
                {
                    return false;
                }
            })
            .WithMessage("ConcurrencyToken must be a Base64 value.");
}
