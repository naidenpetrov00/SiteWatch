using MediatR;

namespace Application.Cameras.Queries;

public class CamerasBySiteQuery : IRequest<List<CamerasDto>>
{
    public Guid SiteId { get; init; }
}

public class CamerasBySiteQueryHandler : IRequestHandler<CamerasBySiteQuery, List<CamerasDto>>
{
    public Guid SiteId { get; init; }

    public Task<List<CamerasDto>> Handle(CamerasBySiteQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}