using Application.SeedWork.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Sites.Services;

public class SiteServices(IApplicationDbContext dbContext, IMapper mapper) : ISiteService
{
    private readonly IApplicationDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;

    public Task<List<SitesDto>> GetSitesByUserAsync(
        Guid userId,
        CancellationToken cancellationToken
    ) =>
        _dbContext
            .Sites.AsNoTracking()
            .Where(site => site.Users.Any(user => user.Id == userId.ToString()))
            .ProjectTo<SitesDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
}
