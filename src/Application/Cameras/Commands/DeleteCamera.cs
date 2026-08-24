using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using MediatR;

namespace Application.Cameras.Commands;

[Authorize(Roles = UserRoleGroups.AdministratorOrWorker)]
public sealed record DeleteCameraCommand(Guid Id) : IRequest;

public sealed class DeleteCameraHandler(ICameraService cameraService) : IRequestHandler<DeleteCameraCommand>
{
    public Task Handle(DeleteCameraCommand request, CancellationToken cancellationToken) =>
        cameraService.DeleteCameraAsync(request.Id, cancellationToken);
}
