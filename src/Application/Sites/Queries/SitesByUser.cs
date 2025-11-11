using MediatR;

namespace Application.Sites.Queries;

public class SitesByUserQuery : IRequest<List<SitesDto>>
{
    public Guid UserId { get; init; }
}

public class SitesByUserQueryHandler : IRequestHandler<SitesByUserQuery, List<SitesDto>>
{
    private readonly ISiteService _siteService;

    public SitesByUserQueryHandler(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public Task<List<SitesDto>> Handle(
        SitesByUserQuery request,
        CancellationToken cancellationToken
    ) => _siteService.GetSitesByUserAsync(request.UserId, cancellationToken);
}
