using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Ardalis.GuardClauses;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Cameras.Queries;

[Authorize(Roles = UserRoles.Administrator)]
public sealed record DashboardCameraByIdQuery(Guid CameraId) : IRequest<DashboardCameraDetailsDto>;

public sealed class DashboardCameraByIdQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<DashboardCameraByIdQuery, DashboardCameraDetailsDto>
{
    public async Task<DashboardCameraDetailsDto> Handle(
        DashboardCameraByIdQuery request,
        CancellationToken cancellationToken)
    {
        var camera = await dbContext.Cameras
            .AsNoTracking()
            .Include(item => item.Site)
            .SingleOrDefaultAsync(item => item.Id == request.CameraId, cancellationToken);

        if (camera is null)
        {
            throw new NotFoundException(nameof(Camera), request.CameraId.ToString());
        }

        return DashboardCameraDetailsDto.From(camera);
    }
}
