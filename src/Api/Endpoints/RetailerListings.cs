using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.RetailerListings;
using Application.SeedWork.Security;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public sealed class RetailerListings : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app
            .MapGroupCustom(customGroupName: "retailer-listings")
            .RequireAuthorization(AuthorizationPolicies.Administrator);

        group.MapGet("/{listingId:guid}", GetListing)
            .WithName("GetRetailerListing")
            .WithSummary("Get one Product–Retailer listing")
            .Produces<RetailerListingDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        group.MapPost(string.Empty, CreateListing)
            .WithName("CreateRetailerListing")
            .WithSummary("Create a Product–Retailer listing with an optional manual price")
            .Produces<RetailerListingDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        group.MapPut("/{listingId:guid}", UpdateListing)
            .WithName("UpdateRetailerListing")
            .WithSummary("Update listing metadata and optionally append a manual price")
            .Produces<RetailerListingDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        group.MapPatch("/{listingId:guid}/activate", ActivateListing)
            .WithName("ActivateRetailerListing")
            .WithSummary("Reactivate a retained Product–Retailer listing")
            .Produces<RetailerListingDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        group.MapPatch("/{listingId:guid}/deactivate", DeactivateListing)
            .WithName("DeactivateRetailerListing")
            .WithSummary("Deactivate a Product–Retailer listing without deleting history")
            .Produces<RetailerListingDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
    }

    private static async Task<Ok<RetailerListingDto>> GetListing(
        IMediator mediator,
        Guid listingId,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Send(
            new RetailerListingByIdQuery(listingId),
            cancellationToken));

    private static async Task<Created<RetailerListingDto>> CreateListing(
        IMediator mediator,
        CreateRetailerListingCommand command,
        CancellationToken cancellationToken)
    {
        var listing = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/retailer-listings/{listing.Id}", listing);
    }

    private static async Task<Ok<RetailerListingDto>> UpdateListing(
        IMediator mediator,
        Guid listingId,
        UpdateRetailerListingCommand command,
        CancellationToken cancellationToken)
    {
        command.ListingId = listingId;
        return TypedResults.Ok(await mediator.Send(command, cancellationToken));
    }

    private static Task<Ok<RetailerListingDto>> ActivateListing(
        IMediator mediator,
        Guid listingId,
        CancellationToken cancellationToken) =>
        SetActive(mediator, listingId, true, cancellationToken);

    private static Task<Ok<RetailerListingDto>> DeactivateListing(
        IMediator mediator,
        Guid listingId,
        CancellationToken cancellationToken) =>
        SetActive(mediator, listingId, false, cancellationToken);

    private static async Task<Ok<RetailerListingDto>> SetActive(
        IMediator mediator,
        Guid listingId,
        bool isActive,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Send(
            new SetRetailerListingActiveCommand(listingId, isActive),
            cancellationToken));
}
