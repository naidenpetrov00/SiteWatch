using Application.Cameras.Queries;
using Application.SeedWork.Interfaces;
using Ardalis.GuardClauses;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Cameras.Services;

public class CameraService(IApplicationDbContext dbContext, IMapper mapper) : ICameraService
{
    private async Task<Camera> _GetCameraAsync(Guid cameraId)
    {
        var camera = await dbContext.Cameras.FindAsync(cameraId);
        Guard.Against.NotFound(cameraId, camera);
        return camera;
    }

    public Task<List<CameraDto>> GetCamerasBySiteIdAsync(Guid siteId, CancellationToken cancellationToken)
        => dbContext.Cameras.AsNoTracking().Where(camera => camera.Site!.Id == siteId)
            .ProjectTo<CameraDto>(mapper.ConfigurationProvider).ToListAsync(cancellationToken);

    public async Task<CameraDto> GetCameraByIdAsync(Guid cameraId, CancellationToken cancellationToken)
    {
        var result = await dbContext.Cameras
            .AsNoTracking()
            .ProjectTo<CameraDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(c => c.Id == cameraId, cancellationToken);

        Guard.Against.NotFound(cameraId, result);

        return result;
    }

    public async Task<Camera> CreateCameraAsync(CameraName cameraName, CameraBrand cameraBrand,
        CancellationToken cancellationToken,
        string? username = null, string? password = null, string? ipAddress = null, int? port = null,
        Guid? siteId = null)
    {
        var camera = Camera.Create(cameraName, cameraBrand, username, password, ipAddress, port, siteId);

        await dbContext.Cameras.AddAsync(camera, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return camera;
    }

    public async Task UpdateAdrressCameraAsync(Guid cameraId, string? ipAddress, int ptzPort,
        CancellationToken cancellationToken)
    {
        var cameraFromDb = await _GetCameraAsync(cameraId);

        cameraFromDb.UpdateIpAddress(ipAddress);
        cameraFromDb.UpdatePtzPort(ptzPort);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}