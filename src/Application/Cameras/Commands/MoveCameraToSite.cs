using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Cameras.Commands;

[Authorize(Roles = UserRoleGroups.AdministratorOrWorker)]
public sealed record MoveCameraToSiteCommand(Guid CameraId, Guid SiteId) : IRequest;

public sealed class MoveCameraToSiteHandler(ICameraService cameraService)
    : IRequestHandler<MoveCameraToSiteCommand>
{
    public Task Handle(MoveCameraToSiteCommand request, CancellationToken cancellationToken) =>
        cameraService.MoveCameraToSiteAsync(request.CameraId, request.SiteId, cancellationToken);
}

public sealed class MoveCameraToSiteValidator : AbstractValidator<MoveCameraToSiteCommand>
{
    public MoveCameraToSiteValidator(IApplicationDbContext dbContext)
    {
        RuleFor(camera => camera.CameraId).NotEmpty();
        RuleFor(camera => camera.SiteId)
            .NotEmpty()
            .MustAsync((siteId, cancellationToken) => dbContext.Sites
                .AsNoTracking()
                .AnyAsync(site => site.Id == siteId, cancellationToken))
            .WithMessage("Site does not exist.");
    }
}
