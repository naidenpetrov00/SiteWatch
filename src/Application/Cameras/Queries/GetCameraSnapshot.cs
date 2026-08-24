using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.Internal;
using FluentValidation;
using MediatR;

namespace Application.Cameras.Queries;

public sealed record GetCameraSnapshotQuery(Guid CameraId) : IRequest<FileResponse>;

public sealed class GetCameraSnapshotQueryValidator : AbstractValidator<GetCameraSnapshotQuery>
{
    public GetCameraSnapshotQueryValidator()
    {
        RuleFor(query => query.CameraId).NotEmpty();
    }
}

public sealed class GetCameraSnapshotQueryHandler(ICameraService cameraService)
    : IRequestHandler<GetCameraSnapshotQuery, FileResponse>
{
    public Task<FileResponse> Handle(
        GetCameraSnapshotQuery request,
        CancellationToken cancellationToken) =>
        cameraService.GetSnapshotAsync(request.CameraId, cancellationToken);
}
