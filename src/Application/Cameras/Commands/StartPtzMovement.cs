using Application.SeedWork.Interfaces;
using FluentValidation;
using MediatR;

namespace Application.Cameras.Commands;

public sealed record StartPtzMovementCommand(Guid CameraId, string Direction) : IRequest;

public sealed class StartPtzMovementCommandValidator : AbstractValidator<StartPtzMovementCommand>
{
    public StartPtzMovementCommandValidator()
    {
        RuleFor(command => command.CameraId).NotEmpty();
        RuleFor(command => command.Direction)
            .NotEmpty()
            .Must(PtzDirectionValidation.IsSupported)
            .WithMessage("Direction must be Up, Down, Left, or Right.");
    }
}

public sealed class StartPtzMovementCommandHandler(ICameraService cameraService)
    : IRequestHandler<StartPtzMovementCommand>
{
    public Task Handle(StartPtzMovementCommand request, CancellationToken cancellationToken) =>
        cameraService.StartPtzMovementAsync(
            request.CameraId,
            PtzDirectionValidation.Normalize(request.Direction),
            cancellationToken);
}
