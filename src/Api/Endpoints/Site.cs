using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.Sites.Queries;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public class Site : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroupCustom();

        group.MapGet("/sitesByUser/{userId}", SitesByUser);
    }

    public async Task<Ok<List<SitesDto>>> SitesByUser(IMediator mediator, [FromQuery] Guid userId)
    {
        var sites = await mediator.Send(new SitesByUserQuery());
        return TypedResults.Ok(sites);
    }
}
