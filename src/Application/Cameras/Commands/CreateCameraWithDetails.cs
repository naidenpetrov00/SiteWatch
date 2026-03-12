using Application.SeedWork.Interfaces;
using Domain.ValueObjects;
using MediatR;

namespace Application.Cameras.Commands;

public class CreateCameraWithDetailsCommand : IRequest<Guid>
{
    public required CameraName CameraName { get; init; }
    public required CameraBrand CameraBrand { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }
    public string? IpAddress { get; init; }
    public int Port { get; init; }
    public Guid SiteId { get; init; }
}

public class CreateCameraWithDetailsHandler(ICameraService cameraService)
    : IRequestHandler<CreateCameraWithDetailsCommand, Guid>
{
    public async Task<Guid> Handle(CreateCameraWithDetailsCommand request, CancellationToken cancellationToken)
    {
        var camera = await cameraService.CreateCameraAsync(request.CameraName, request.CameraBrand,
            cancellationToken, request.Username, request.Password, request.IpAddress, request.Port, request.SiteId);

        return camera.Id;
    }
}