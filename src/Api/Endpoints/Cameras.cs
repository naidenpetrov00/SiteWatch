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
        group.MapPatch("/{cameraId:guid}/connections", UpdateCameraIpAndPort).RequireAuthorization();
        group.MapGet("/site/{siteId:guid}/cameras", CamerasBySite).RequireAuthorization();
        group.MapPost("/withDetails", CreateCameraWithDetails).RequireAuthorization();
        group.MapPost(string.Empty, CreateDashboardCamera)
            .RequireAuthorization(AuthorizationPolicies.Administrator);
        group.MapPut("/{cameraId:guid}", UpdateDashboardCamera)
            .RequireAuthorization(AuthorizationPolicies.Administrator);
        group.MapDelete("/{cameraId:guid}", DeleteCamera)
            .RequireAuthorization(AuthorizationPolicies.Administrator);
        dashboardGroup.MapGet("/cameras", GetDashboardCameras);
        dashboardGroup.MapGet("/cameras/{cameraId:guid}", GetDashboardCamera);
    }

    private static async Task<Ok<CameraDto>> CameraById(IMediator mediator, Guid cameraId)
    {
        var camera = await mediator.Send(new CameraByIdQuery { CameraId = cameraId });
        return TypedResults.Ok(camera);
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
}
