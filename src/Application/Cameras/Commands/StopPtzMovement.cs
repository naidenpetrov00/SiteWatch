using Application.SeedWork.Interfaces;
using FluentValidation;
using MediatR;

namespace Application.Cameras.Commands;

public sealed record StopPtzMovementCommand(Guid CameraId, string Direction) : IRequest;

public sealed class StopPtzMovementCommandValidator : AbstractValidator<StopPtzMovementCommand>
{
    public StopPtzMovementCommandValidator()
    {
        RuleFor(command => command.CameraId).NotEmpty();
        RuleFor(command => command.Direction)
            .NotEmpty()
            .Must(PtzDirectionValidation.IsSupported)
            .WithMessage("Direction must be Up, Down, Left, or Right.");
    }
}

public sealed class StopPtzMovementCommandHandler(ICameraService cameraService)
    : IRequestHandler<StopPtzMovementCommand>
{
    public Task Handle(StopPtzMovementCommand request, CancellationToken cancellationToken) =>
        cameraService.StopPtzMovementAsync(
            request.CameraId,
            PtzDirectionValidation.Normalize(request.Direction),
            cancellationToken);
}
