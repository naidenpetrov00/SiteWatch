using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.SeedWork.Models;
using Application.SeedWork.Security;
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
        var dashboardGroup = app
            .MapGroupCustom(customGroupName: "dashboard")
            .RequireAuthorization(AuthorizationPolicies.Administrator);

        group
            .MapPost(string.Empty, CreateSite)
            .RequireAuthorization(AuthorizationPolicies.Administrator);
        group.MapGet("/sitesByUser/{userId:guid}", SitesByUser).RequireAuthorization();
        dashboardGroup.MapGet("/sites", GetDashboardSites);
        dashboardGroup.MapGet("/sites/media-policy-presets", GetMediaPolicyPresets);
        dashboardGroup.MapGet("/sites/search", SearchDashboardSites);
        dashboardGroup.MapGet("/sites/{siteId:guid}", GetDashboardSite);
        dashboardGroup.MapPut("/sites/{siteId:guid}", UpdateDashboardSite);
    }

    private static async Task<Ok<List<SitesDto>>> SitesByUser(
        IMediator mediator,
        [AsParameters] SitesByUserQuery query,
        CancellationToken cancellationToken
    )
    {
        var sites = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(sites);
    }

    private static async Task<IResult> CreateSite(
        IMediator mediator,
        CreateSiteCommand command,
        CancellationToken cancellationToken)
    {
        var siteId = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/dashboard/sites/{siteId}", new { id = siteId });
    }

    private static async Task<Ok<PagedResult<DashboardSiteDto>>> GetDashboardSites(
        IMediator mediator,
        [AsParameters] DashboardSitesQuery query,
        CancellationToken cancellationToken
    )
    {
        var sites = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(sites);
    }

    private static async Task<Ok<IReadOnlyList<SiteMediaPolicyPresetDto>>> GetMediaPolicyPresets(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var presets = await mediator.Send(new SiteMediaPolicyPresetsQuery(), cancellationToken);
        return TypedResults.Ok(presets);
    }

    private static async Task<Ok<DashboardSiteDto>> GetDashboardSite(
        IMediator mediator,
        Guid siteId,
        CancellationToken cancellationToken
    )
    {
        var site = await mediator.Send(
            new DashboardSiteByIdQuery { SiteId = siteId },
            cancellationToken);
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
        UpdateSiteCommand command,
        CancellationToken cancellationToken
    )
    {
        command.Id = siteId;
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }
}
