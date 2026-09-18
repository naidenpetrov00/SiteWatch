using Api.SeedWork;
using Application.Offers.Commands;
using Application.Offers.Queries;
using Application.SeedWork.Models;
using Application.SeedWork.Security;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public sealed class Offers : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app
            .MapGroup("/sites/{siteId:guid}/offers")
            .RequireAuthorization(AuthorizationPolicies.Administrator)
            .WithTags("Offers");

        group.MapPost(string.Empty, CreateOffer)
            .WithName("CreateSiteOffer")
            .WithSummary("Create a draft offer for a site")
            .Produces<OfferCreatedResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status404NotFound);
        group.MapGet(string.Empty, GetSiteOffers)
            .WithName("GetSiteOffers")
            .WithSummary("Get a filtered and paged list of offers for a site")
            .Produces<PagedResult<OfferSummaryDto>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);
        group.MapGet("/{offerId:guid}", GetOffer)
            .WithName("GetSiteOffer")
            .WithSummary("Get an offer in the context of its site")
            .Produces<OfferDetailsDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        group.MapGet("/{offerId:guid}/activity-catalog", GetActivityCatalog)
            .WithName("GetOfferActivityCatalog")
            .WithSummary("Browse or search the Activity Catalog for an Offer")
            .Produces<IReadOnlyList<OfferActivityCatalogNodeDto>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);
        group.MapGet(
                "/{offerId:guid}/activity-catalog/{activityId:guid}",
                GetActivityCandidate)
            .WithName("GetOfferActivityCandidate")
            .WithSummary("Load an active Activity Catalog item before adding it to an Offer")
            .Produces<OfferActivityCandidateDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        group.MapPost("/{offerId:guid}/activities", AddActivity)
            .WithName("AddOfferActivity")
            .WithSummary("Add an Activity Catalog snapshot to a draft Offer")
            .Produces<OfferActivityCreatedResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        group.MapDelete(
                "/{offerId:guid}/activities/{offerActivityId:guid}",
                RemoveActivity)
            .WithName("RemoveOfferActivity")
            .WithSummary("Remove a selected activity from a draft Offer")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        group.MapPut(
                "/{offerId:guid}/activities/{offerActivityId:guid}/section-measurements",
                UpdateActivityMeasurements)
            .WithName("UpdateOfferActivityMeasurements")
            .WithSummary("Replace every section measurement for a selected Offer activity")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        group.MapPut("/{offerId:guid}", UpdateOfferMetadata)
            .WithName("UpdateSiteOfferMetadata")
            .WithSummary("Update the editable metadata of a draft offer")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        group.MapPatch("/{offerId:guid}/archive", ArchiveOffer)
            .WithName("ArchiveSiteOffer")
            .WithSummary("Archive an offer without deleting it")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<Created<OfferCreatedResponse>> CreateOffer(
        IMediator mediator,
        Guid siteId,
        CancellationToken cancellationToken)
    {
        var offerId = await mediator.Send(
            new CreateOfferCommand { SiteId = siteId },
            cancellationToken);
        return TypedResults.Created(
            $"/sites/{siteId}/offers/{offerId}",
            new OfferCreatedResponse(offerId));
    }

    private static async Task<Ok<PagedResult<OfferSummaryDto>>> GetSiteOffers(
        IMediator mediator,
        [AsParameters] SiteOffersQuery query,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await mediator.Send(query, cancellationToken));
    }

    private static async Task<Ok<OfferDetailsDto>> GetOffer(
        IMediator mediator,
        Guid siteId,
        Guid offerId,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Send(
            new OfferByIdQuery(siteId, offerId),
            cancellationToken));

    private static async Task<Ok<IReadOnlyList<OfferActivityCatalogNodeDto>>>
        GetActivityCatalog(
            IMediator mediator,
            Guid siteId,
            Guid offerId,
            string? searchTerm,
            CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Send(
            new OfferActivityCatalogQuery
            {
                SiteId = siteId,
                OfferId = offerId,
                SearchTerm = searchTerm
            },
            cancellationToken));

    private static async Task<Ok<OfferActivityCandidateDto>> GetActivityCandidate(
        IMediator mediator,
        Guid siteId,
        Guid offerId,
        Guid activityId,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Send(
            new OfferActivityCandidateQuery(siteId, offerId, activityId),
            cancellationToken));

    private static async Task<Created<OfferActivityCreatedResponse>> AddActivity(
        IMediator mediator,
        Guid siteId,
        Guid offerId,
        AddOfferActivityCommand command,
        CancellationToken cancellationToken)
    {
        command.SiteId = siteId;
        command.OfferId = offerId;
        var offerActivityId = await mediator.Send(command, cancellationToken);
        return TypedResults.Created(
            $"/sites/{siteId}/offers/{offerId}",
            new OfferActivityCreatedResponse(offerActivityId));
    }

    private static async Task<NoContent> RemoveActivity(
        IMediator mediator,
        Guid siteId,
        Guid offerId,
        Guid offerActivityId,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new RemoveOfferActivityCommand(siteId, offerId, offerActivityId),
            cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> UpdateActivityMeasurements(
        IMediator mediator,
        Guid siteId,
        Guid offerId,
        Guid offerActivityId,
        UpdateOfferActivityMeasurementsCommand command,
        CancellationToken cancellationToken)
    {
        command.SiteId = siteId;
        command.OfferId = offerId;
        command.OfferActivityId = offerActivityId;
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> UpdateOfferMetadata(
        IMediator mediator,
        Guid siteId,
        Guid offerId,
        UpdateOfferMetadataCommand command,
        CancellationToken cancellationToken)
    {
        command.SiteId = siteId;
        command.OfferId = offerId;
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> ArchiveOffer(
        IMediator mediator,
        Guid siteId,
        Guid offerId,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new ArchiveOfferCommand(siteId, offerId),
            cancellationToken);
        return TypedResults.NoContent();
    }

    /// <summary>Represents the identifier of a newly created offer.</summary>
    public sealed record OfferCreatedResponse(Guid Id);

    /// <summary>Represents the identifier of a selected Offer activity snapshot.</summary>
    public sealed record OfferActivityCreatedResponse(Guid Id);
}
