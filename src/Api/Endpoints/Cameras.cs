using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.Cameras.Commands;
using Application.Cameras.Queries;
using Application.SeedWork.Models;
using Application.SeedWork.Security;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public class Cameras : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroupCustom(customGroupName: "cameras");
        var dashboardGroup = app
            .MapGroupCustom(customGroupName: "dashboard")
            .RequireAuthorization(AuthorizationPolicies.Administrator);

        group.MapGet("/{cameraId:guid}", CameraById).RequireAuthorization();
        group.MapGet("/{cameraId:guid}/snapshot", GetCameraSnapshot)
            .RequireAuthorization()
            .WithName("GetCameraSnapshot")
            .WithSummary("Get a current camera snapshot")
            .WithDescription("Returns a JPEG snapshot retrieved from the configured Dahua camera.")
            .Produces(StatusCodes.Status200OK, contentType: "image/jpeg")
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status502BadGateway);
        group.MapPost("/{cameraId:guid}/ptz/start", StartPtzMovement)
            .RequireAuthorization()
            .WithName("StartCameraPtzMovement")
            .WithSummary("Start moving a PTZ camera")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status502BadGateway);
        group.MapPost("/{cameraId:guid}/ptz/stop", StopPtzMovement)
            .RequireAuthorization()
            .WithName("StopCameraPtzMovement")
            .WithSummary("Stop moving a PTZ camera")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status502BadGateway);
        group.MapPost("/{cameraId:guid}/ptz/relative", MovePtzRelatively)
            .RequireAuthorization()
            .WithName("MoveCameraPtzRelatively")
            .WithSummary("Move a PTZ camera by normalized offsets")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status502BadGateway);
        group.MapPatch("/{cameraId:guid}/connections", UpdateCameraIpAndPort).RequireAuthorization();
        group.MapGet("/site/{siteId:guid}/cameras", CamerasBySite).RequireAuthorization();
        group.MapPost("/withDetails", CreateCameraWithDetails).RequireAuthorization();
        group.MapPost(string.Empty, CreateDashboardCamera)
            .RequireAuthorization(AuthorizationPolicies.AdministratorOrWorker);
        group.MapPut("/{cameraId:guid}", UpdateDashboardCamera)
            .RequireAuthorization(AuthorizationPolicies.Administrator);
        group.MapPatch("/{cameraId:guid}/site", MoveCameraToSite)
            .RequireAuthorization(AuthorizationPolicies.AdministratorOrWorker)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);
        group.MapDelete("/{cameraId:guid}", DeleteCamera)
            .RequireAuthorization(AuthorizationPolicies.AdministratorOrWorker);
        dashboardGroup.MapGet("/cameras", GetDashboardCameras);
        dashboardGroup.MapGet("/cameras/{cameraId:guid}", GetDashboardCamera);
    }

    private static async Task<Ok<CameraDto>> CameraById(IMediator mediator, Guid cameraId)
    {
        var camera = await mediator.Send(new CameraByIdQuery { CameraId = cameraId });
        return TypedResults.Ok(camera);
    }

    private static async Task<FileStreamHttpResult> GetCameraSnapshot(
        IMediator mediator,
        HttpContext httpContext,
        Guid cameraId,
        CancellationToken cancellationToken)
    {
        var snapshot = await mediator.Send(
            new GetCameraSnapshotQuery(cameraId),
            cancellationToken);
        httpContext.Response.Headers.CacheControl = "private, no-store";
        httpContext.Response.Headers["X-Content-Type-Options"] = "nosniff";

        return TypedResults.File(snapshot.Stream, snapshot.ContentType);
    }

    private static async Task<NoContent> StartPtzMovement(
        IMediator mediator,
        Guid cameraId,
        PtzDirectionRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new StartPtzMovementCommand(cameraId, request.Direction),
            cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> StopPtzMovement(
        IMediator mediator,
        Guid cameraId,
        PtzDirectionRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new StopPtzMovementCommand(cameraId, request.Direction),
            cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> MovePtzRelatively(
        IMediator mediator,
        Guid cameraId,
        MovePtzRelativelyRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new MovePtzRelativelyCommand(
                cameraId,
                request.Horizontal,
                request.Vertical,
                request.Zoom),
            cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> UpdateCameraIpAndPort(IMediator mediator, Guid cameraId,
        UpdateCameraIpAndPort command)
    {
        command.Id = cameraId;
        await mediator.Send(command);

        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<CameraDto>>> CamerasBySite(IMediator mediator, Guid siteId)
    {
        var cameras = await mediator.Send(new CamerasBySiteQuery { SiteId = siteId });
        return TypedResults.Ok(cameras);
    }

    private static async Task<IResult> CreateCameraWithDetails(IMediator mediator,
        CreateCameraWithDetails command)
    {
        var cameraId = await mediator.Send(command);
        return TypedResults.Created($"/cameraById/{cameraId}", new { id = cameraId });
    }

    private static async Task<IResult> CreateDashboardCamera(
        IMediator mediator,
        CreateDashboardCameraCommand command,
        CancellationToken cancellationToken)
    {
        var cameraId = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/cameras/{cameraId}", new { id = cameraId });
    }

    private static async Task<NoContent> UpdateDashboardCamera(
        IMediator mediator,
        Guid cameraId,
        UpdateDashboardCameraCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = cameraId;
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> MoveCameraToSite(
        IMediator mediator,
        Guid cameraId,
        MoveCameraToSiteRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new MoveCameraToSiteCommand(cameraId, request.SiteId), cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteCamera(
        IMediator mediator,
        Guid cameraId,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteCameraCommand(cameraId), cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<PagedResult<DashboardCameraDto>>> GetDashboardCameras(
        IMediator mediator,
        [AsParameters] DashboardCamerasQuery query,
        CancellationToken cancellationToken)
    {
        var cameras = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(cameras);
    }

    private static async Task<Ok<DashboardCameraDetailsDto>> GetDashboardCamera(
        IMediator mediator,
        Guid cameraId,
        CancellationToken cancellationToken)
    {
        var camera = await mediator.Send(new DashboardCameraByIdQuery(cameraId), cancellationToken);
        return TypedResults.Ok(camera);
    }

    /// <summary>Specifies a supported PTZ movement direction.</summary>
    public sealed record PtzDirectionRequest
    {
        public required string Direction { get; init; }
    }

    /// <summary>Specifies normalized relative PTZ movement values.</summary>
    public sealed record MovePtzRelativelyRequest
    {
        public double Horizontal { get; init; }
        public double Vertical { get; init; }
        public double Zoom { get; init; }
    }

    /// <summary>Specifies the site to which a camera is reassigned.</summary>
    public sealed record MoveCameraToSiteRequest(Guid SiteId);
}
