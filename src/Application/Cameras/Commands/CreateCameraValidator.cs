using FluentValidation;
using Domain.SeedWork.Enums;
using Domain.ValueObjects;

namespace Application.Cameras.Commands;

internal class CreateCameraValidator : AbstractValidator<CreateCameraCommand>
{
    internal CreateCameraValidator()
    {
        RuleFor(cc => cc.CameraName)
            .NotNull()
            .SetValidator(new CameraNameValidator()!);

        RuleFor(cc => cc.CameraBrand)
            .NotNull()
            .SetValidator(new CameraBrandValidator()!);
    }
}

internal sealed class CameraNameValidator : AbstractValidator<CameraName>
{
    internal CameraNameValidator()
    {
        RuleFor(cn => cn.Value)
            .NotEmpty()
            .Length(1, 100);
    }
}

internal sealed class CameraBrandValidator : AbstractValidator<CameraBrand>
{
    internal CameraBrandValidator()
    {
        RuleFor(cb => cb.Brand)
            .IsInEnum()
            .NotEqual(Brand.Unknown);

        RuleFor(cb => cb.Model)
            .NotEmpty()
            .Length(1, 100);
    }
}
