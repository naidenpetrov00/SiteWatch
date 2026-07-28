using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.SeedWork.Models;
using Application.Sites.Commands;
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

        group.MapPost(string.Empty, CreateSite).RequireAuthorization();
        group.MapGet("/sitesByUser/{userId:guid}", SitesByUser).RequireAuthorization();
        dashboardGroup.MapGet("/sites", GetDashboardSites);
        dashboardGroup.MapGet("/sites/search", SearchDashboardSites);
        dashboardGroup.MapGet("/sites/{siteId:guid}", GetDashboardSite);
        dashboardGroup.MapPut("/sites/{siteId:guid}", UpdateDashboardSite);
    }

    private static async Task<Ok<List<SitesDto>>> SitesByUser(
        IMediator mediator,
        [AsParameters] SitesByUserQuery query
    )
    {
        var sites = await mediator.Send(query);
        return TypedResults.Ok(sites);
    }

    private static async Task<IResult> CreateSite(IMediator mediator, CreateSiteCommand command)
    {
        var siteId = await mediator.Send(command);
        return TypedResults.Created($"/dashboard/sites/{siteId}", new { id = siteId });
    }

    private static async Task<Ok<PagedResult<DashboardSiteDto>>> GetDashboardSites(
        IMediator mediator,
        [AsParameters] DashboardSitesQuery query
    )
    {
        var sites = await mediator.Send(query);
        return TypedResults.Ok(sites);
    }

    private static async Task<Ok<DashboardSiteDto>> GetDashboardSite(
        IMediator mediator,
        Guid siteId
    )
    {
        var site = await mediator.Send(new DashboardSiteByIdQuery { SiteId = siteId });
        return TypedResults.Ok(site);
    }

    private static async Task<Ok<List<SiteLookupDto>>> SearchDashboardSites(
        IMediator mediator,
        [AsParameters] SiteSearchQuery query,
        CancellationToken cancellationToken)
    {
        var sites = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(sites);
    }

    private static async Task<NoContent> UpdateDashboardSite(
        IMediator mediator,
        Guid siteId,
        UpdateSiteCommand command
    )
    {
        command.Id = siteId;
        await mediator.Send(command);
        return TypedResults.NoContent();
    }
}
