using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.Cameras.Commands;
using Application.Cameras.Queries;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public class Cameras : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroupCustom(customGroupName: "cameras");
        group.MapGet("/{cameraId:guid}", CameraById).RequireAuthorization();
        group.MapPatch("/{cameraId:guid}/connections", UpdateCameraIpAndPort).RequireAuthorization();
        group.MapGet("/site/{siteId:guid}/cameras", CamerasBySite).RequireAuthorization();
        group.MapPost("/withDetails", CreateCameraWithDetails).RequireAuthorization();
    }

    private static async Task<Ok<CameraDto>> CameraById(IMediator mediator, Guid cameraId)
    {
        var camera = await mediator.Send(new CameraByIdQuery { CameraId = cameraId });
        return TypedResults.Ok(camera);
    }

    private async Task<NoContent> UpdateCameraIpAndPort(IMediator mediator, Guid cameraId,
        UpdateCameraIpAndPortCommand command)
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
        CreateCameraWithDetailsCommand command)
    {
        var cameraId = await mediator.Send(command);
        return TypedResults.Created($"/cameraById/{cameraId}", new { id = cameraId });
    }
}