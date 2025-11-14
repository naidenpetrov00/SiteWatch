using Application.Sites.Queries;

public interface ISiteService
{
    Task<List<SitesDto>> GetSitesByUserAsync(Guid userId, CancellationToken cancellationToken);
}
