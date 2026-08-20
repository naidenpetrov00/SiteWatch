using System.Globalization;
using System.Net;
using Application.Cameras;
using Application.Cameras.Queries;
using Application.Cameras.Commands;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.Internal;
using Ardalis.GuardClauses;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Domain.SeedWork.Enums;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Cameras.Services;

public sealed class CameraService(IApplicationDbContext dbContext, IMapper mapper) : ICameraService
{
    private const int CameraChannel = 1;
    private const int PtzSpeed = 5;
    private static readonly TimeSpan CameraRequestTimeout = TimeSpan.FromSeconds(10);

    private async Task<Camera> GetCameraAsync(Guid cameraId, CancellationToken cancellationToken)
    {
        var camera = await dbContext.Cameras.FindAsync([cameraId], cancellationToken);
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

    public async Task<FileResponse> GetSnapshotAsync(
        Guid cameraId,
        CancellationToken cancellationToken)
    {
        var camera = await GetCameraAsync(cameraId, cancellationToken);
        var snapshotUri = BuildCameraUri(
            camera,
            "/cgi-bin/snapshot.cgi",
            [
                new("channel", CameraChannel.ToString(CultureInfo.InvariantCulture)),
                new("type", "0"),
            ]);

        using var client = CreateDahuaClient(camera);
        using var response = await SendGetAsync(client, snapshotUri, cancellationToken);
        if (!response.IsSuccessStatusCode ||
            !string.Equals(
                response.Content.Headers.ContentType?.MediaType,
                "image/jpeg",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new CameraCommunicationException();
        }

        try
        {
            var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            return new FileResponse(new MemoryStream(content, writable: false), "image/jpeg");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new CameraCommunicationException();
        }
        catch (HttpRequestException exception)
        {
            throw new CameraCommunicationException(exception);
        }
    }

    public Task StartPtzMovementAsync(
        Guid cameraId,
        string direction,
        CancellationToken cancellationToken) =>
        SendPtzCommandAsync(
            cameraId,
            "start",
            direction,
            "0",
            PtzSpeed.ToString(CultureInfo.InvariantCulture),
            "0",
            cancellationToken);

    public Task StopPtzMovementAsync(
        Guid cameraId,
        string direction,
        CancellationToken cancellationToken) =>
        SendPtzCommandAsync(
            cameraId,
            "stop",
            direction,
            "0",
            "0",
            "0",
            cancellationToken);

    public async Task MovePtzRelativelyAsync(
        Guid cameraId,
        double horizontal,
        double vertical,
        double zoom,
        CancellationToken cancellationToken)
    {
        var camera = await GetCameraAsync(cameraId, cancellationToken);
        var ptzUri = BuildCameraUri(
            camera,
            "/cgi-bin/ptz.cgi",
            [
                new("action", "moveRelatively"),
                new("channel", CameraChannel.ToString(CultureInfo.InvariantCulture)),
                new("arg1", horizontal.ToString(CultureInfo.InvariantCulture)),
                new("arg2", vertical.ToString(CultureInfo.InvariantCulture)),
                new("arg3", zoom.ToString(CultureInfo.InvariantCulture)),
            ]);

        await SendPtzRequestAsync(camera, ptzUri, cancellationToken);
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
        var cameraFromDb = await GetCameraAsync(cameraId, cancellationToken);

        cameraFromDb.UpdateIpAddress(ipAddress);
        cameraFromDb.UpdatePtzPort(ptzPort);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Guid> CreateDashboardCameraAsync(CameraUpsertDto request, CancellationToken cancellationToken)
    {
        var camera = Camera.Create(
            request.Name,
            CameraBrand.Create(ParseBrand(request.Brand), request.Model),
            request.Username,
            request.Password,
            request.IpAddress,
            request.RtspPort,
            request.SiteId,
            ParseProtocol(request.Protocol));
        camera.UpdatePtzPort(request.PtzPort);

        await dbContext.Cameras.AddAsync(camera, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return camera.Id;
    }

    public async Task UpdateDashboardCameraAsync(Guid cameraId, CameraUpsertDto request, CancellationToken cancellationToken)
    {
        var camera = await GetCameraAsync(cameraId, cancellationToken);
        camera.UpdateName(request.Name);
        camera.UpdateBrand(CameraBrand.Create(ParseBrand(request.Brand), request.Model));
        camera.UpdateUsername(request.Username);
        camera.UpdatePassword(request.Password);
        camera.UpdateIpAddress(request.IpAddress);
        camera.UpdateRtspPort(request.RtspPort);
        camera.UpdatePtzPort(request.PtzPort);
        camera.UpdateProtocol(ParseProtocol(request.Protocol));
        camera.AssignToSite(request.SiteId);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteCameraAsync(Guid cameraId, CancellationToken cancellationToken)
    {
        var deletedRows = await dbContext.Cameras
            .Where(camera => camera.Id == cameraId)
            .ExecuteDeleteAsync(cancellationToken);

        if (deletedRows == 0)
        {
            throw new NotFoundException(nameof(Camera), cameraId.ToString());
        }
    }

    private static Brand ParseBrand(string value) =>
        Enum.Parse<Brand>(value, ignoreCase: true);

    private static CameraProtocol ParseProtocol(string value) =>
        Enum.Parse<CameraProtocol>(value, ignoreCase: true);

    private async Task SendPtzCommandAsync(
        Guid cameraId,
        string action,
        string direction,
        string arg1,
        string arg2,
        string arg3,
        CancellationToken cancellationToken)
    {
        var camera = await GetCameraAsync(cameraId, cancellationToken);
        var ptzUri = BuildCameraUri(
            camera,
            "/cgi-bin/ptz.cgi",
            [
                new("action", action),
                new("channel", CameraChannel.ToString(CultureInfo.InvariantCulture)),
                new("code", direction),
                new("arg1", arg1),
                new("arg2", arg2),
                new("arg3", arg3),
            ]);

        await SendPtzRequestAsync(camera, ptzUri, cancellationToken);
    }

    private static async Task SendPtzRequestAsync(
        Camera camera,
        Uri requestUri,
        CancellationToken cancellationToken)
    {
        using var client = CreateDahuaClient(camera);
        using var response = await SendGetAsync(client, requestUri, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new CameraCommunicationException();
        }

        try
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!string.Equals(body.Trim(), "OK", StringComparison.OrdinalIgnoreCase))
            {
                throw new CameraCommunicationException();
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new CameraCommunicationException();
        }
        catch (HttpRequestException exception)
        {
            throw new CameraCommunicationException(exception);
        }
    }

    private static async Task<HttpResponseMessage> SendGetAsync(
        HttpClient client,
        Uri requestUri,
        CancellationToken cancellationToken)
    {
        try
        {
            return await client.GetAsync(
                requestUri,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new CameraCommunicationException();
        }
        catch (HttpRequestException exception)
        {
            throw new CameraCommunicationException(exception);
        }
    }

    private static HttpClient CreateDahuaClient(Camera camera)
    {
        EnsureDahuaConnectionDetails(camera);

        var handler = new HttpClientHandler
        {
            Credentials = new NetworkCredential(camera.Username!, camera.Password!),
            PreAuthenticate = true,
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
        };

        return new HttpClient(handler, disposeHandler: true)
        {
            Timeout = CameraRequestTimeout,
        };
    }

    private static Uri BuildCameraUri(
        Camera camera,
        string path,
        IReadOnlyCollection<KeyValuePair<string, string>> parameters)
    {
        EnsureDahuaConnectionDetails(camera);

        try
        {
            var query = string.Join(
                "&",
                parameters.Select(parameter =>
                    $"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(parameter.Value)}"));
            var scheme = camera.Protocol == CameraProtocol.Http
                ? Uri.UriSchemeHttp
                : Uri.UriSchemeHttps;

            return new UriBuilder(scheme, camera.IpAddress!, camera.PtzPort!.Value, path)
            {
                Query = query,
            }.Uri;
        }
        catch (UriFormatException exception)
        {
            throw new CameraCommunicationException(exception);
        }
    }

    private static void EnsureDahuaConnectionDetails(Camera camera)
    {
        if (camera.CameraBrand.Brand != Brand.Dahua ||
            string.IsNullOrWhiteSpace(camera.IpAddress) ||
            camera.PtzPort is null or < 1 or > 65535 ||
            string.IsNullOrWhiteSpace(camera.Username) ||
            string.IsNullOrWhiteSpace(camera.Password))
        {
            throw new CameraCommunicationException();
        }
    }
}
