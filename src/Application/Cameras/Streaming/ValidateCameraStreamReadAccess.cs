using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Cameras.Streaming;

public sealed record ValidateCameraStreamReadAccessQuery(
    Guid SiteId,
    Guid CameraId,
    string UserId) : IRequest<bool>;

public sealed class ValidateCameraStreamReadAccessQueryHandler(
    IApplicationDbContext dbContext,
    IIdentityService identityService)
    : IRequestHandler<ValidateCameraStreamReadAccessQuery, bool>
{
    public async Task<bool> Handle(
        ValidateCameraStreamReadAccessQuery request,
        CancellationToken cancellationToken)
    {
        var relationshipExists = await dbContext.Cameras
            .AsNoTracking()
            .AnyAsync(
                camera => camera.Id == request.CameraId
                    && camera.SiteId == request.SiteId,
                cancellationToken);

        return relationshipExists
            && await identityService.IsInRoleAsync(
                request.UserId,
                UserRoles.Administrator);
    }
}
