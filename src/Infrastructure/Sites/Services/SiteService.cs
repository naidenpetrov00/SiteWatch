using Application.SeedWork.Interfaces;
using Application.Sites.Queries;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Sites.Services;

public class SiteService(IApplicationDbContext dbContext, IMapper mapper) : ISiteService
{
    public async Task<List<SitesDto>> GetSitesByUserAsync(
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        var sites = await dbContext
            .Sites.AsNoTracking()
            .Where(site => site.Users.Any(user => user.Id == userId.ToString()))
            .ToListAsync(cancellationToken);

        return mapper.Map<List<SitesDto>>(sites);
    }
}
