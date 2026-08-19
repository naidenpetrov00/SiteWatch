using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Cameras.Commands;

[Authorize(Roles = UserRoles.Administrator)]
public sealed record CreateDashboardCameraCommand : CameraUpsertDto, IRequest<Guid>;

public sealed class CreateDashboardCameraHandler(ICameraService cameraService)
    : IRequestHandler<CreateDashboardCameraCommand, Guid>
{
    public Task<Guid> Handle(CreateDashboardCameraCommand request, CancellationToken cancellationToken) =>
        cameraService.CreateDashboardCameraAsync(request, cancellationToken);
}

public sealed class CreateDashboardCameraValidator
    : CameraUpsertValidator<CreateDashboardCameraCommand>
{
    public CreateDashboardCameraValidator(IApplicationDbContext dbContext)
    {
        RuleFor(camera => camera.SiteId)
            .MustAsync((siteId, cancellationToken) => dbContext.Sites
                .AsNoTracking()
                .AnyAsync(site => site.Id == siteId, cancellationToken))
            .WithMessage("Site does not exist.");
    }
}
