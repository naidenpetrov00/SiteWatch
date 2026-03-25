using System.Text.Json.Serialization;
using Application.SeedWork.Interfaces;
using MediatR;

namespace Application.Cameras.Commands;

public class UpdateCameraIpAndPortCommand : IRequest
{
    [JsonIgnore] public Guid Id { get; set; }
    public string? IpAddress { get; init; }
    public int PtzPort { get; init; }
}

public class UpdateCameraIpAndPortHandler(ICameraService cameraService) : IRequestHandler<UpdateCameraIpAndPortCommand>
{
    private readonly ICameraService _cameraService = cameraService;

    public async Task Handle(UpdateCameraIpAndPortCommand request, CancellationToken cancellationToken)
    {
        await _cameraService.UpdateAdrressCameraAsync(request.Id, request.IpAddress, request.PtzPort,
            cancellationToken);
    }
}