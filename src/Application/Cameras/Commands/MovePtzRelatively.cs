using Application.SeedWork.Interfaces;
using FluentValidation;
using MediatR;

namespace Application.Cameras.Commands;

public sealed record MovePtzRelativelyCommand(
    Guid CameraId,
    double Horizontal,
    double Vertical,
    double Zoom) : IRequest;

public sealed class MovePtzRelativelyCommandValidator : AbstractValidator<MovePtzRelativelyCommand>
{
    public MovePtzRelativelyCommandValidator()
    {
        RuleFor(command => command.CameraId).NotEmpty();
        RuleFor(command => command.Horizontal).InclusiveBetween(-1, 1);
        RuleFor(command => command.Vertical).InclusiveBetween(-1, 1);
        RuleFor(command => command.Zoom).InclusiveBetween(-1, 1);
    }
}

public sealed class MovePtzRelativelyCommandHandler(ICameraService cameraService)
    : IRequestHandler<MovePtzRelativelyCommand>
{
    public Task Handle(MovePtzRelativelyCommand request, CancellationToken cancellationToken) =>
        cameraService.MovePtzRelativelyAsync(
            request.CameraId,
            request.Horizontal,
            request.Vertical,
            request.Zoom,
            cancellationToken);
}
