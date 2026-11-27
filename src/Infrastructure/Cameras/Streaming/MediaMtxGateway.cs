using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Cameras.Streaming;
using Domain.Entities;
using Domain.SeedWork.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Cameras.Streaming;

public sealed class MediaMtxGateway(
    HttpClient httpClient,
    IOptions<MediaMtxOptions> options,
    IMediaMtxAccessTicketService ticketService,
    MediaMtxPathReconciliationState reconciliationState,
    ILogger<MediaMtxGateway> logger) : IMediaMtxGateway
{
    private const int CameraChannel = 1;
    private const int MaxReconciliationAttempts = 3;
    private const string SourceOnDemandStartTimeout = "10s";
    private const string SourceOnDemandCloseAfter = "10s";
    private const string RtspTransport = "tcp";

    private readonly Uri _controlApiBaseUri = CreateBaseUri(options.Value.ControlApiBaseUrl);
    private readonly Uri _publicWhepBaseUri = CreateBaseUri(options.Value.PublicWhepBaseUrl);

    public async Task<PreparedCameraStream> ReconcileCameraPathAsync(
        Camera camera,
        CameraStreamProfile profile,
        CancellationToken cancellationToken)
    {
        var path = CameraStreamPaths.Create(camera.Id, profile);
        var source = BuildDahuaSourceUri(camera, profile);
        var sourceFingerprint = SHA256.HashData(
            Encoding.UTF8.GetBytes(source.AbsoluteUri));
        var desired = MediaMtxPathConfiguration.Create(source.AbsoluteUri);
        var pathLock = reconciliationState.GetPathLock(path);

        await pathLock.WaitAsync(cancellationToken);
        try
        {
            for (var attempt = 0; attempt < MaxReconciliationAttempts; attempt++)
            {
                var existing = await GetPathConfigurationAsync(path, cancellationToken);
                if (existing is null)
                {
                    if (await TryAddPathAsync(path, desired, cancellationToken))
                    {
                        reconciliationState.SetFingerprint(path, sourceFingerprint);
                        return CreatePreparedStream(path);
                    }

                    continue;
                }

                if (ConfigurationMatches(existing, source)
                    && reconciliationState.MatchesFingerprint(path, sourceFingerprint))
                {
                    return CreatePreparedStream(path);
                }

                if (await TryReplacePathAsync(path, desired, cancellationToken))
                {
                    reconciliationState.SetFingerprint(path, sourceFingerprint);
                    return CreatePreparedStream(path);
                }
            }

            logger.LogWarning(
                "MediaMTX path reconciliation did not converge for camera {CameraId} and path {Path}.",
                camera.Id,
                path);
            throw new MediaMtxGatewayUnavailableException();
        }
        finally
        {
            pathLock.Release();
        }
    }

    private PreparedCameraStream CreatePreparedStream(string path) =>
        new(path, new Uri(_publicWhepBaseUri, $"{path}/whep"));

    private async Task<MediaMtxPathConfiguration?> GetPathConfigurationAsync(
        string path,
        CancellationToken cancellationToken)
    {
        using var request = CreateRequest(
            HttpMethod.Get,
            $"v3/config/paths/get/{Uri.EscapeDataString(path)}");

        using var response = await SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            LogUnexpectedStatus("get", path, response.StatusCode);
            throw new MediaMtxGatewayUnavailableException();
        }

        try
        {
            return await response.Content.ReadFromJsonAsync<MediaMtxPathConfiguration>(
                cancellationToken)
                ?? throw new MediaMtxGatewayUnavailableException();
        }
        catch (Exception exception) when (
            exception is JsonException or NotSupportedException)
        {
            throw new MediaMtxGatewayUnavailableException(exception);
        }
    }

    private async Task<bool> TryAddPathAsync(
        string path,
        MediaMtxPathConfiguration desired,
        CancellationToken cancellationToken)
    {
        using var request = CreateRequest(
            HttpMethod.Post,
            $"v3/config/paths/add/{Uri.EscapeDataString(path)}",
            desired);
        using var response = await SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            return true;
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            return false;
        }

        LogUnexpectedStatus("add", path, response.StatusCode);
        throw new MediaMtxGatewayUnavailableException();
    }

    private async Task<bool> TryReplacePathAsync(
        string path,
        MediaMtxPathConfiguration desired,
        CancellationToken cancellationToken)
    {
        using var request = CreateRequest(
            HttpMethod.Post,
            $"v3/config/paths/replace/{Uri.EscapeDataString(path)}",
            desired);
        using var response = await SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            return true;
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        LogUnexpectedStatus("replace", path, response.StatusCode);
        throw new MediaMtxGatewayUnavailableException();
    }

    private HttpRequestMessage CreateRequest(
        HttpMethod method,
        string relativePath,
        MediaMtxPathConfiguration? content = null)
    {
        var request = new HttpRequestMessage(
            method,
            new Uri(_controlApiBaseUri, relativePath));
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            ticketService.CreateControlApiTicket());

        if (content is not null)
        {
            request.Content = JsonContent.Create(content);
        }

        return request;
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            return await httpClient.SendAsync(request, cancellationToken);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("A MediaMTX Control API request timed out.");
            throw new MediaMtxGatewayUnavailableException(exception);
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning("A MediaMTX Control API request failed.");
            throw new MediaMtxGatewayUnavailableException(exception);
        }
    }

    private void LogUnexpectedStatus(
        string operation,
        string path,
        HttpStatusCode statusCode) =>
        logger.LogWarning(
            "MediaMTX Control API {Operation} failed for path {Path} with status {StatusCode}.",
            operation,
            path,
            (int)statusCode);

    private static bool ConfigurationMatches(
        MediaMtxPathConfiguration existing,
        Uri desiredSource) =>
        SourcesMatchIgnoringCredentials(existing.Source, desiredSource)
        && existing.SourceOnDemand
        && string.Equals(
            existing.SourceOnDemandStartTimeout,
            SourceOnDemandStartTimeout,
            StringComparison.Ordinal)
        && string.Equals(
            existing.SourceOnDemandCloseAfter,
            SourceOnDemandCloseAfter,
            StringComparison.Ordinal)
        && string.Equals(existing.RtspTransport, RtspTransport, StringComparison.OrdinalIgnoreCase)
        && !existing.Record;

    private static bool SourcesMatchIgnoringCredentials(string? existingSource, Uri desiredSource)
    {
        if (string.IsNullOrWhiteSpace(existingSource)
            || !Uri.TryCreate(StripUserInfo(existingSource), UriKind.Absolute, out var existing))
        {
            return false;
        }

        return string.Equals(existing.Scheme, desiredSource.Scheme, StringComparison.OrdinalIgnoreCase)
            && string.Equals(existing.Host, desiredSource.Host, StringComparison.OrdinalIgnoreCase)
            && existing.Port == desiredSource.Port
            && string.Equals(existing.AbsolutePath, desiredSource.AbsolutePath, StringComparison.Ordinal)
            && string.Equals(existing.Query, desiredSource.Query, StringComparison.Ordinal);
    }

    private static string StripUserInfo(string source)
    {
        var schemeEnd = source.IndexOf("://", StringComparison.Ordinal);
        if (schemeEnd < 0)
        {
            return source;
        }

        var authorityStart = schemeEnd + 3;
        var authorityEnd = source.IndexOfAny(['/', '?', '#'], authorityStart);
        if (authorityEnd < 0)
        {
            authorityEnd = source.Length;
        }

        var authority = source.AsSpan(authorityStart, authorityEnd - authorityStart);
        var relativeAt = authority.LastIndexOf('@');
        return relativeAt < 0
            ? source
            : string.Concat(
                source.AsSpan(0, authorityStart),
                source.AsSpan(authorityStart + relativeAt + 1));
    }

    private static Uri BuildDahuaSourceUri(
        Camera camera,
        CameraStreamProfile profile)
    {
        if (camera.CameraBrand.Brand != Brand.Dahua
            || string.IsNullOrWhiteSpace(camera.IpAddress)
            || Uri.CheckHostName(camera.IpAddress) == UriHostNameType.Unknown
            || camera.RtspPort is null or < 1 or > 65535
            || string.IsNullOrWhiteSpace(camera.Username)
            || string.IsNullOrWhiteSpace(camera.Password))
        {
            throw new CameraStreamConfigurationException();
        }

        try
        {
            var subtype = profile == CameraStreamProfile.Main ? 0 : 1;
            return new UriBuilder(
                "rtsp",
                camera.IpAddress,
                camera.RtspPort.Value,
                "/cam/realmonitor")
            {
                UserName = camera.Username,
                Password = camera.Password,
                Query = $"channel={CameraChannel}&subtype={subtype}",
            }.Uri;
        }
        catch (UriFormatException)
        {
            throw new CameraStreamConfigurationException();
        }
    }

    private static Uri CreateBaseUri(string value)
    {
        var builder = new UriBuilder(value);
        if (!builder.Path.EndsWith("/", StringComparison.Ordinal))
        {
            builder.Path += "/";
        }

        return builder.Uri;
    }

    private sealed class MediaMtxPathConfiguration
    {
        [JsonPropertyName("source")]
        public string Source { get; init; } = string.Empty;

        [JsonPropertyName("sourceOnDemand")]
        public bool SourceOnDemand { get; init; }

        [JsonPropertyName("sourceOnDemandStartTimeout")]
        public string SourceOnDemandStartTimeout { get; init; } = string.Empty;

        [JsonPropertyName("sourceOnDemandCloseAfter")]
        public string SourceOnDemandCloseAfter { get; init; } = string.Empty;

        [JsonPropertyName("rtspTransport")]
        public string RtspTransport { get; init; } = string.Empty;

        [JsonPropertyName("record")]
        public bool Record { get; init; }

        public static MediaMtxPathConfiguration Create(string source) => new()
        {
            Source = source,
            SourceOnDemand = true,
            SourceOnDemandStartTimeout = MediaMtxGateway.SourceOnDemandStartTimeout,
            SourceOnDemandCloseAfter = MediaMtxGateway.SourceOnDemandCloseAfter,
            RtspTransport = MediaMtxGateway.RtspTransport,
            Record = false,
        };
    }
}
