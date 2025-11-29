using Application.SeedWork.Interfaces;
using MediatR;

namespace Application.Cameras.Queries;

public class CamerasBySiteQuery : IRequest<List<CamerasDto>>
{
    public Guid SiteId { get; init; }
}

public class CamerasBySiteQueryHandler(ICameraService cameraService)
    : IRequestHandler<CamerasBySiteQuery, List<CamerasDto>>
{
    public Task<List<CamerasDto>> Handle(CamerasBySiteQuery request, CancellationToken cancellationToken) =>
        cameraService.GetCamerasBySiteIdAsync(request.SiteId, cancellationToken);
}