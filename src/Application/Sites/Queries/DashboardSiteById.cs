using Application.SeedWork.Interfaces;
using Application.SeedWork.Queries;
using Ardalis.GuardClauses;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Sites.Queries;

public sealed record DashboardSiteByIdQuery : IRequest<DashboardSiteDto>
{
    public Guid SiteId { get; init; }
}

public sealed class DashboardSiteByIdQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<DashboardSiteByIdQuery, DashboardSiteDto>
{
    public async Task<DashboardSiteDto> Handle(
        DashboardSiteByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var site = await dbContext.Sites
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == request.SiteId, cancellationToken);

        if (site is null)
        {
            throw new NotFoundException(nameof(Site), request.SiteId.ToString());
        }

        return DashboardSiteDto.From(site);
    }
}
