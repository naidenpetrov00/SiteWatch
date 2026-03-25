using Ardalis.GuardClauses;
using FluentValidation;

namespace Application.Cameras.Commands;

internal class UpdateCameraIpAndPortValidator : AbstractValidator<UpdateCameraIpAndPortCommand>
{
    internal UpdateCameraIpAndPortValidator()
    {
        RuleFor(uc => uc.Id).NotEmpty();
        RuleFor(uc => uc.PtzPort).GreaterThan(0);
        RuleFor(uc => uc.IpAddress).NotEmpty();
    }
}