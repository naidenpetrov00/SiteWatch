using Application.Cameras.Queries;

namespace Application.SeedWork.Interfaces;

public interface ICameraService
{
    Task<List<CameraDto>> GetCamerasBySiteIdAsync(Guid siteId, CancellationToken cancellationToken);
    Task<CameraDto> GetCameraByIdAsync(Guid requestId, CancellationToken cancellationToken);
}