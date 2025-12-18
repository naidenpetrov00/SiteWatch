using Application.SeedWork.Interfaces;
using MediatR;

namespace Application.Cameras.Queries;

public class CamerasBySiteQuery : IRequest<List<CameraDto>>
{
    public Guid SiteId { get; init; }
}

public class CamerasBySiteQueryHandler(ICameraService cameraService)
    : IRequestHandler<CamerasBySiteQuery, List<CameraDto>>
{
    public Task<List<CameraDto>> Handle(CamerasBySiteQuery request, CancellationToken cancellationToken) =>
        cameraService.GetCamerasBySiteIdAsync(request.SiteId, cancellationToken);
}