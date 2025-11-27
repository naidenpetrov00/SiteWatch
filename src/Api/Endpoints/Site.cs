using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.Sites.Queries;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public class Site : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroupCustom();

        group.MapGet("/sitesByUser/{userId:guid}", SitesByUser).RequireAuthorization();
    }

    private static async Task<Ok<List<SitesDto>>> SitesByUser(
        IMediator mediator,
        [AsParameters] SitesByUserQuery query
    )
    {
        var sites = await mediator.Send(query);
        return TypedResults.Ok(sites);
    }
}