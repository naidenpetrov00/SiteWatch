using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Application.SeedWork.Queries;
using Application.SeedWork.Security;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Cameras.Queries;

[Authorize(Roles = UserRoles.Administrator)]
public sealed partial class DashboardCamerasQuery : TableQueryRequest, IRequest<PagedResult<DashboardCameraDto>>
{
    public string? NumberId { get; set; }
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? IpAddress { get; set; }
    public string? SiteName { get; set; }
}

public sealed class DashboardCamerasQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<DashboardCamerasQuery, PagedResult<DashboardCameraDto>>
{
    public async Task<PagedResult<DashboardCameraDto>> Handle(
        DashboardCamerasQuery request,
        CancellationToken cancellationToken)
    {
        var result = await dbContext.Cameras
            .AsNoTracking()
            .Include(camera => camera.Site)
            .ToPagedResultAsync<Camera, Camera, DashboardCamerasQuery>(
                request,
                DashboardCamerasQuery.Table,
                query => query,
                cancellationToken);

        return new PagedResult<DashboardCameraDto>(
            result.Items.Select(DashboardCameraDto.From).ToList(),
            result.FilteredCount,
            result.TotalCount);
    }
}
