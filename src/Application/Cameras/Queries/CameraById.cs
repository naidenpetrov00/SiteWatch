using Application.SeedWork.Interfaces;
using MediatR;

namespace Application.Cameras.Queries;

public class CameraByIdQuery : IRequest<CameraDto>
{
    public Guid CameraId { get; init; }
}

public class CameraByIdQueryHandler(ICameraService cameraService) : IRequestHandler<CameraByIdQuery, CameraDto>
{
    public Task<CameraDto> Handle(CameraByIdQuery request, CancellationToken cancellationToken)
        => cameraService.GetCameraByIdAsync(request.CameraId, cancellationToken);
}