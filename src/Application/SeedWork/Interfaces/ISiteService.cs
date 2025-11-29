using Application.Sites.Queries;

namespace Application.SeedWork.Interfaces;

public interface ISiteService
{
    Task<List<SitesDto>> GetSitesByUserAsync(Guid userId, CancellationToken cancellationToken);
}