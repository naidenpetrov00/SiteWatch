using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Ardalis.GuardClauses;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Cameras.Queries;

[Authorize(Roles = UserRoles.Administrator)]
public sealed record DashboardSiteCamerasQuery(Guid SiteId)
    : IRequest<IReadOnlyList<DashboardSiteCameraDto>>;

public sealed class DashboardSiteCamerasQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<DashboardSiteCamerasQuery, IReadOnlyList<DashboardSiteCameraDto>>
{
    public async Task<IReadOnlyList<DashboardSiteCameraDto>> Handle(
        DashboardSiteCamerasQuery request,
        CancellationToken cancellationToken)
    {
        var siteExists = await dbContext.Sites
            .AsNoTracking()
            .AnyAsync(site => site.Id == request.SiteId, cancellationToken);

        if (!siteExists)
        {
            throw new NotFoundException(nameof(Site), request.SiteId.ToString());
        }

        var cameras = await dbContext.Cameras
            .AsNoTracking()
            .Where(camera => camera.SiteId == request.SiteId)
            .OrderBy(camera => camera.NumberId)
            .ThenBy(camera => camera.Id)
            .Select(camera => new
            {
                camera.Id,
                camera.NumberId,
                Name = camera.CameraName.Value,
                camera.CameraBrand.Brand,
                camera.CameraBrand.Model,
            })
            .ToListAsync(cancellationToken);

        return cameras
            .Select(camera => new DashboardSiteCameraDto(
                camera.Id,
                camera.NumberId,
                camera.Name,
                camera.Brand.ToString(),
                camera.Model))
            .ToList();
    }
}

/// <summary>Represents safe camera identity details for a dashboard site.</summary>
public sealed record DashboardSiteCameraDto(
    Guid Id,
    int NumberId,
    string Name,
    string Brand,
    string Model);
