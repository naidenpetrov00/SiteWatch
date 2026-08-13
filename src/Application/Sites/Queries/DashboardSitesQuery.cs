using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Application.SeedWork.Queries;
using Application.SeedWork.Security;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Sites.Queries;

[Authorize(Roles = UserRoles.Administrator)]
public sealed partial class DashboardSitesQuery : TableQueryRequest, IRequest<PagedResult<DashboardSiteDto>>
{
    public string? NumberId { get; set; }
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
}

public sealed class DashboardSitesQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<DashboardSitesQuery, PagedResult<DashboardSiteDto>>
{
    public async Task<PagedResult<DashboardSiteDto>> Handle(
        DashboardSitesQuery request,
        CancellationToken cancellationToken
    )
    {
        var result = await dbContext.Sites
            .AsNoTracking()
            .ToPagedResultAsync<Site, Site, DashboardSitesQuery>(
                request,
                DashboardSitesQuery.Table,
                query => query,
                cancellationToken
            );

        return new PagedResult<DashboardSiteDto>(
            result.Items.Select(DashboardSiteDto.From).ToList(),
            result.FilteredCount,
            result.TotalCount
        );
    }
}

/// <summary>Represents a site returned by the dashboard site table.</summary>
public sealed record DashboardSiteDto(
    Guid Id,
    int NumberId,
    string Name,
    string Address,
    SiteMediaPolicyDto MediaPolicy)
{
    public static DashboardSiteDto From(Site site) => new(
        site.Id,
        site.NumberId,
        site.Name.Value,
        site.Address.Value,
        SiteMediaPolicyDto.From(site.MediaPolicy));
}
