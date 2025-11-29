using Application.SeedWork.Interfaces;
using Application.Sites.Queries;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Sites.Services;

public class SiteService(IApplicationDbContext dbContext, IMapper mapper) : ISiteService
{
    public Task<List<SitesDto>> GetSitesByUserAsync(
        Guid userId,
        CancellationToken cancellationToken
    ) =>
        dbContext
            .Sites.AsNoTracking()
            .Where(site => site.Users.Any(user => user.Id == userId.ToString()))
            .ProjectTo<SitesDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
}