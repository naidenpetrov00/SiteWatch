using Application.SeedWork.Interfaces;
using MediatR;

namespace Application.Sites.Videos.Queries;

public sealed record GetVideosIdsBySiteIdQuery : IRequest<List<SiteVideoIdsDto>>
{
    public Guid SiteId { get; init; }
}

public sealed class GetVideosIdsBySiteIdHandler(IVideosService videosService)
    : IRequestHandler<GetVideosIdsBySiteIdQuery, List<SiteVideoIdsDto>>
{
    public Task<List<SiteVideoIdsDto>> Handle(
        GetVideosIdsBySiteIdQuery request,
        CancellationToken cancellationToken
    ) => videosService.GetVideosIdsBySiteId(request.SiteId);
}
