using MediatR;

namespace Application.Sites.Queries;

public class SitesByUserQuery : IRequest<List<SitesDto>>
{
    public Guid UserId { get; init; }
}

public class SitesByUserQueryHandler(ISiteService siteService) : IRequestHandler<SitesByUserQuery, List<SitesDto>>
{
    public Task<List<SitesDto>> Handle(
        SitesByUserQuery request,
        CancellationToken cancellationToken
    ) => siteService.GetSitesByUserAsync(request.UserId, cancellationToken);
}