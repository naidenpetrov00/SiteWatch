using Application.Sites.Queries;
using Application.Sites.Commands;

namespace Application.SeedWork.Interfaces;

public interface ISiteService
{
    Task<Guid> CreateAsync(CreateSiteCommand request, CancellationToken cancellationToken);
    Task<List<SitesDto>> GetSitesByUserAsync(Guid userId, CancellationToken cancellationToken);
    Task UpdateAsync(UpdateSiteCommand request, CancellationToken cancellationToken);
}
