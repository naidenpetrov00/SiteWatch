using System.Text.Json;
using System.Text.Json.Serialization;
using Api.SeedWork;
using Application.Cameras.Streaming;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public sealed class MediaMtxAuthorization : EndpointGroupBase
{
    private const int MaxRequestBodyBytes = 16 * 1024;

    public override void Map(WebApplication app)
    {
        app.MapPost("/internal/mediamtx/auth", Authorize)
            .AllowAnonymous()
            .ExcludeFromDescription();
    }

    private static async Task<Results<NoContent, UnauthorizedHttpResult>> Authorize(
        HttpRequest httpRequest,
        HttpResponse httpResponse,
        IMediaMtxAccessTicketService ticketService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        httpResponse.Headers.CacheControl = "no-store";
        try
        {
            return await AuthorizeCore(
                httpRequest,
                ticketService,
                mediator,
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return TypedResults.Unauthorized();
        }
    }

    private static async Task<Results<NoContent, UnauthorizedHttpResult>> AuthorizeCore(
        HttpRequest httpRequest,
        IMediaMtxAccessTicketService ticketService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var request = await ReadRequestAsync(httpRequest, cancellationToken);
        if (request is null)
        {
            return TypedResults.Unauthorized();
        }

        if (string.Equals(request.Action, "api", StringComparison.Ordinal))
        {
            if (!ticketService.TryReadControlApiTicket(request.Token))
            {
                return TypedResults.Unauthorized();
            }

            return TypedResults.NoContent();
        }

        if (!string.Equals(request.Action, "read", StringComparison.Ordinal)
            || !string.Equals(request.Protocol, "webrtc", StringComparison.Ordinal)
            || !ticketService.TryReadViewerReadTicket(request.Token, out var accessTicket)
            || accessTicket is null
            || !string.Equals(request.Path, accessTicket.Path, StringComparison.Ordinal))
        {
            return TypedResults.Unauthorized();
        }

        var authorized = await mediator.Send(
            new ValidateCameraStreamReadAccessQuery(
                accessTicket.SiteId,
                accessTicket.CameraId,
                accessTicket.UserId),
            cancellationToken);

        if (!authorized)
        {
            return TypedResults.Unauthorized();
        }

        return TypedResults.NoContent();
    }

    private static async Task<MediaMtxAuthorizationRequest?> ReadRequestAsync(
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ContentLength is <= 0 or > MaxRequestBodyBytes)
        {
            return null;
        }

        await using var body = new MemoryStream();
        var buffer = new byte[4096];
        var totalBytes = 0;

        while (true)
        {
            var bytesRead = await request.Body.ReadAsync(buffer, cancellationToken);
            if (bytesRead == 0)
            {
                break;
            }

            totalBytes += bytesRead;
            if (totalBytes > MaxRequestBodyBytes)
            {
                return null;
            }

            await body.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
        }

        if (totalBytes == 0)
        {
            return null;
        }

        body.Position = 0;
        try
        {
            return await JsonSerializer.DeserializeAsync<MediaMtxAuthorizationRequest>(
                body,
                cancellationToken: cancellationToken);
        }
        catch (Exception exception) when (exception is JsonException or NotSupportedException)
        {
            return null;
        }
    }

    private sealed record MediaMtxAuthorizationRequest
    {
        [JsonPropertyName("token")]
        public string Token { get; init; } = string.Empty;

        [JsonPropertyName("action")]
        public string Action { get; init; } = string.Empty;

        [JsonPropertyName("path")]
        public string Path { get; init; } = string.Empty;

        [JsonPropertyName("protocol")]
        public string Protocol { get; init; } = string.Empty;
    }
}
