using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.Cameras.Queries;
using MediatR;

namespace Api.Endpoints;

public class Camera : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroupCustom();
    }

    private static async Task CamerasBySite(IMediator mediator, [AsParameters] CamerasBySiteQuery query)
    {
        var cameras = await mediator.Send(query);
    }
}