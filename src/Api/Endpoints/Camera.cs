using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.Cameras.Queries;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public class Camera : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroupCustom();
        group.MapGet("/cameraById/{cameraId:guid}", CameraById).RequireAuthorization();
        group.MapGet("/camerasBySite/{siteId:guid}", CamerasBySite).RequireAuthorization();
    }

    private static async Task<Ok<CameraDto>> CameraById(IMediator mediator,
        [AsParameters] CameraByIdQuery query)
    {
        var camera = await mediator.Send(query);
        return TypedResults.Ok(camera);
    }

    private static async Task<Ok<List<CameraDto>>> CamerasBySite(IMediator mediator,
        [AsParameters] CamerasBySiteQuery query)
    {
        var cameras = await mediator.Send(query);
        return TypedResults.Ok(cameras);
    }
}