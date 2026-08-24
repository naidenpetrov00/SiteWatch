using Application.Cameras.Commands;
using Application.Cameras.Queries;
using Application.SeedWork.Models.Internal;
using Domain.Entities;
using Domain.ValueObjects;

namespace Application.SeedWork.Interfaces;

public interface ICameraService
{
    Task<List<CameraDto>> GetCamerasBySiteIdAsync(Guid siteId, CancellationToken cancellationToken);
    Task<CameraDto> GetCameraByIdAsync(Guid requestId, CancellationToken cancellationToken);
    Task<FileResponse> GetSnapshotAsync(Guid cameraId, CancellationToken cancellationToken);
    Task StartPtzMovementAsync(Guid cameraId, string direction, CancellationToken cancellationToken);
    Task StopPtzMovementAsync(Guid cameraId, string direction, CancellationToken cancellationToken);
    Task MovePtzRelativelyAsync(
        Guid cameraId,
        double horizontal,
        double vertical,
        double zoom,
        CancellationToken cancellationToken);

    Task<Camera> CreateCameraAsync(
        CameraName cameraName,
        CameraBrand cameraBrand,
        CancellationToken cancellationToken,
        string? username = null,
        string? password = null,
        string? ipAddress = null,
        int? port = null,
        Guid? siteId = null
    );

    Task UpdateAdrressCameraAsync(Guid cameraId, string? ipAddress, int ptzPort, CancellationToken cancellationToken);
    Task<Guid> CreateDashboardCameraAsync(CameraUpsertDto request, CancellationToken cancellationToken);
    Task UpdateDashboardCameraAsync(Guid cameraId, CameraUpsertDto request, CancellationToken cancellationToken);
    Task DeleteCameraAsync(Guid cameraId, CancellationToken cancellationToken);
}
