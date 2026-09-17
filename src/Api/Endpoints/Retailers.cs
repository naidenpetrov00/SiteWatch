using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.Retailers.Commands;
using Application.Retailers.Queries;
using Application.SeedWork.Models;
using Application.SeedWork.Security;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public sealed class Retailers : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app
            .MapGroupCustom(customGroupName: "retailers")
            .RequireAuthorization(AuthorizationPolicies.Administrator);
        var dashboardGroup = app
            .MapGroupCustom(customGroupName: "dashboard")
            .RequireAuthorization(AuthorizationPolicies.Administrator);

        group.MapPost(string.Empty, CreateRetailer)
            .WithName("CreateRetailer")
            .WithSummary("Create an active retailer storefront");
        group.MapGet("/{retailerId:guid}", GetRetailer)
            .WithName("GetRetailer")
            .WithSummary("Get a retailer storefront by ID");
        group.MapPut("/{retailerId:guid}", UpdateRetailer)
            .WithName("UpdateRetailer")
            .WithSummary("Update a retailer storefront and legal company");
        group.MapPatch("/{retailerId:guid}/activate", ActivateRetailer)
            .WithName("ActivateRetailer")
            .WithSummary("Activate a retailer storefront");
        group.MapPatch("/{retailerId:guid}/deactivate", DeactivateRetailer)
            .WithName("DeactivateRetailer")
            .WithSummary("Deactivate a retailer storefront");
        dashboardGroup.MapGet("/retailers", GetDashboardRetailers)
            .WithName("GetDashboardRetailers")
            .WithSummary("Get a filtered and paged retailer catalog");
        dashboardGroup.MapGet("/retailers/search", SearchDashboardRetailers)
            .WithName("SearchDashboardRetailers")
            .WithSummary("Search active retailers for assignment lookup");
    }

    private static async Task<IResult> CreateRetailer(
        IMediator mediator,
        CreateRetailerCommand command,
        CancellationToken cancellationToken)
    {
        var retailerId = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/retailers/{retailerId}", new { id = retailerId });
    }

    private static async Task<Ok<RetailerDetailsDto>> GetRetailer(
        IMediator mediator,
        Guid retailerId,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Send(
            new RetailerByIdQuery(retailerId),
            cancellationToken));

    private static async Task<NoContent> UpdateRetailer(
        IMediator mediator,
        Guid retailerId,
        UpdateRetailerCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = retailerId;
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static Task<NoContent> ActivateRetailer(
        IMediator mediator,
        Guid retailerId,
        CancellationToken cancellationToken) =>
        SetRetailerActive(mediator, retailerId, true, cancellationToken);

    private static Task<NoContent> DeactivateRetailer(
        IMediator mediator,
        Guid retailerId,
        CancellationToken cancellationToken) =>
        SetRetailerActive(mediator, retailerId, false, cancellationToken);

    private static async Task<NoContent> SetRetailerActive(
        IMediator mediator,
        Guid retailerId,
        bool isActive,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new SetRetailerActiveCommand(retailerId, isActive),
            cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<PagedResult<RetailerTableDto>>> GetDashboardRetailers(
        IMediator mediator,
        [AsParameters] DashboardRetailersQuery query,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Send(query, cancellationToken));

    private static async Task<Ok<List<RetailerLookupDto>>> SearchDashboardRetailers(
        IMediator mediator,
        [AsParameters] RetailerSearchQuery query,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Send(query, cancellationToken));
}
