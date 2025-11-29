using Application.Cameras.Queries;
using Application.SeedWork.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Cameras;

public class CameraService(IApplicationDbContext dbContext, IMapper mapper) : ICameraService
{
    public Task<List<CamerasDto>> GetCamerasBySiteIdAsync(Guid siteId, CancellationToken cancellationToken)
        => dbContext.Cameras.AsNoTracking().Where(camera => camera.Site!.Id == siteId)
            .ProjectTo<CamerasDto>(mapper.ConfigurationProvider).ToListAsync(cancellationToken);
}