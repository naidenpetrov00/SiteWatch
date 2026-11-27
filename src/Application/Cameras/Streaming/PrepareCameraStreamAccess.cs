using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Ardalis.GuardClauses;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Cameras.Streaming;

[Authorize(Roles = UserRoles.Administrator)]
public sealed record PrepareCameraStreamAccessCommand(
    Guid SiteId,
    Guid CameraId,
    string Profile) : IRequest<PreparedCameraStream>;

public sealed class PrepareCameraStreamAccessCommandValidator
    : AbstractValidator<PrepareCameraStreamAccessCommand>
{
    public PrepareCameraStreamAccessCommandValidator()
    {
        RuleFor(request => request.Profile)
            .NotEmpty()
            .Must(value => CameraStreamProfiles.TryParse(value, out _))
            .WithMessage("Profile must be either main or sub.");
    }
}

public sealed class PrepareCameraStreamAccessCommandHandler(
    IApplicationDbContext dbContext,
    IMediaMtxGateway mediaMtxGateway)
    : IRequestHandler<PrepareCameraStreamAccessCommand, PreparedCameraStream>
{
    public async Task<PreparedCameraStream> Handle(
        PrepareCameraStreamAccessCommand request,
        CancellationToken cancellationToken)
    {
        var camera = await dbContext.Cameras
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == request.CameraId && item.SiteId == request.SiteId,
                cancellationToken);

        if (camera is null)
        {
            throw new NotFoundException(nameof(Camera), request.CameraId.ToString());
        }

        if (!CameraStreamProfiles.TryParse(request.Profile, out var profile))
        {
            throw new CameraStreamConfigurationException();
        }

        return await mediaMtxGateway.ReconcileCameraPathAsync(
            camera,
            profile,
            cancellationToken);
    }
}
