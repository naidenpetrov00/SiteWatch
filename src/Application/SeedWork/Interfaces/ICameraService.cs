using Application.Cameras.Queries;

namespace Application.SeedWork.Interfaces;

public interface ICameraService
{
    Task<List<CamerasDto>> GetCamerasBySiteIdAsync(Guid siteId, CancellationToken cancellationToken);
}