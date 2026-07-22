using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.SeedWork.Models;
using Application.Sites.Queries;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints.Sites;

public class Sites : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroupCustom();
        var dashboardGroup = app.MapGroupCustom(customGroupName: "dashboard").RequireAuthorization();

        group.MapGet("/sitesByUser/{userId:guid}", SitesByUser).RequireAuthorization();
        dashboardGroup.MapGet("/sites", GetDashboardSites);
    }

    private static async Task<Ok<List<SitesDto>>> SitesByUser(
        IMediator mediator,
        [AsParameters] SitesByUserQuery query
    )
    {
        var sites = await mediator.Send(query);
        return TypedResults.Ok(sites);
    }

    private static async Task<Ok<PagedResult<DashboardSiteDto>>> GetDashboardSites(
        IMediator mediator,
        [AsParameters] DashboardSitesQuery query
    )
    {
        var sites = await mediator.Send(query);
        return TypedResults.Ok(sites);
    }
}
