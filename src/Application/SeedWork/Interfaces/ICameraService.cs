using Application.Cameras.Queries;
using Domain.ValueObjects;

namespace Application.SeedWork.Interfaces;

public interface ICameraService
{
    Task<List<CameraDto>> GetCamerasBySiteIdAsync(Guid siteId, CancellationToken cancellationToken);
    Task<CameraDto> GetCameraByIdAsync(Guid requestId, CancellationToken cancellationToken);

    Task<bool> CreateCameraAsync(
        CameraName cameraName,
        CameraBrand cameraBrand,
        CancellationToken cancellationToken,
        string? username = null,
        string? password = null,
        string? ipAddress = null,
        int? port = null,
        Guid? siteId = null
    );
}