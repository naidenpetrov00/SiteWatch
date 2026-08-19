using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Cameras.Commands;

[Authorize(Roles = UserRoles.Administrator)]
public sealed record UpdateDashboardCameraCommand : CameraUpsertDto, IRequest
{
    public Guid Id { get; set; }
}

public sealed class UpdateDashboardCameraHandler(ICameraService cameraService)
    : IRequestHandler<UpdateDashboardCameraCommand>
{
    public Task Handle(UpdateDashboardCameraCommand request, CancellationToken cancellationToken) =>
        cameraService.UpdateDashboardCameraAsync(request.Id, request, cancellationToken);
}

public sealed class UpdateDashboardCameraValidator
    : CameraUpsertValidator<UpdateDashboardCameraCommand>
{
    public UpdateDashboardCameraValidator(IApplicationDbContext dbContext)
    {
        RuleFor(camera => camera.Id).NotEmpty();
        RuleFor(camera => camera.SiteId)
            .MustAsync((siteId, cancellationToken) => dbContext.Sites
                .AsNoTracking()
                .AnyAsync(site => site.Id == siteId, cancellationToken))
            .WithMessage("Site does not exist.");
    }
}
