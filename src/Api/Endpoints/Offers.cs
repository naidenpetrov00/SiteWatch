using Api.SeedWork;
using Application.Offers.Commands;
using Application.Offers.Queries;
using Application.Offers.Pricing;
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
        group.MapGet("/{offerId:guid}/pricing", GetPricingMatrix)
            .WithName("GetOfferPricingMatrix")
            .WithSummary("Get the Product-by-Retailer pricing matrix for an Offer")
            .Produces<OfferPricingMatrixDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        group.MapPost("/{offerId:guid}/pricing/retailers", AddPricingRetailer)
            .WithName("AddOfferPricingRetailer")
            .WithSummary("Add an active Retailer to a draft Offer comparison matrix")
            .Produces<OfferPricingMatrixDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        group.MapDelete(
                "/{offerId:guid}/pricing/retailers/{retailerId:guid}",
                RemovePricingRetailer)
            .WithName("RemoveOfferPricingRetailer")
            .WithSummary("Remove an unused Retailer from a draft Offer comparison matrix")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        group.MapPut(
                "/{offerId:guid}/pricing/products/{offerProductLineId:guid}/retailers/{retailerId:guid}/current-price",
                RecordManualPrice)
            .WithName("RecordManualOfferRetailerPrice")
            .WithSummary("Record the current manual EUR price for an Offer matrix cell")
            .Produces<OfferRetailerPriceCellDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        group.MapPut(
                "/{offerId:guid}/pricing/products/{offerProductLineId:guid}/selection",
                SelectProductPrice)
            .WithName("SelectOfferProductPrice")
            .WithSummary("Snapshot the latest selected Retailer price for an Offer Product")
            .Produces<OfferPricingProductRowDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        group.MapDelete(
                "/{offerId:guid}/pricing/products/{offerProductLineId:guid}/selection",
                ClearProductPrice)
            .WithName("ClearOfferProductPrice")
            .WithSummary("Clear the selected price snapshot for a draft Offer Product")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
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

    private static async Task<Ok<OfferPricingMatrixDto>> GetPricingMatrix(
        IMediator mediator,
        Guid siteId,
        Guid offerId,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Send(
            new OfferPricingMatrixQuery(siteId, offerId),
            cancellationToken));

    private static async Task<Created<OfferPricingMatrixDto>> AddPricingRetailer(
        IMediator mediator,
        Guid siteId,
        Guid offerId,
        AddOfferRetailerCommand command,
        CancellationToken cancellationToken)
    {
        command.SiteId = siteId;
        command.OfferId = offerId;
        var matrix = await mediator.Send(command, cancellationToken);
        return TypedResults.Created(
            $"/sites/{siteId}/offers/{offerId}/pricing/retailers/{command.RetailerId}",
            matrix);
    }

    private static async Task<NoContent> RemovePricingRetailer(
        IMediator mediator,
        Guid siteId,
        Guid offerId,
        Guid retailerId,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new RemoveOfferRetailerCommand(siteId, offerId, retailerId),
            cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<OfferRetailerPriceCellDto>> RecordManualPrice(
        IMediator mediator,
        Guid siteId,
        Guid offerId,
        Guid offerProductLineId,
        Guid retailerId,
        RecordManualRetailerPriceCommand command,
        CancellationToken cancellationToken)
    {
        command.SiteId = siteId;
        command.OfferId = offerId;
        command.OfferProductLineId = offerProductLineId;
        command.RetailerId = retailerId;
        return TypedResults.Ok(await mediator.Send(command, cancellationToken));
    }

    private static async Task<Ok<OfferPricingProductRowDto>> SelectProductPrice(
        IMediator mediator,
        Guid siteId,
        Guid offerId,
        Guid offerProductLineId,
        SelectOfferProductPriceCommand command,
        CancellationToken cancellationToken)
    {
        command.SiteId = siteId;
        command.OfferId = offerId;
        command.OfferProductLineId = offerProductLineId;
        return TypedResults.Ok(await mediator.Send(command, cancellationToken));
    }

    private static async Task<NoContent> ClearProductPrice(
        IMediator mediator,
        Guid siteId,
        Guid offerId,
        Guid offerProductLineId,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new ClearOfferProductPriceCommand(siteId, offerId, offerProductLineId),
            cancellationToken);
        return TypedResults.NoContent();
    }

    /// <summary>Represents the identifier of a newly created offer.</summary>
    public sealed record OfferCreatedResponse(Guid Id);

    /// <summary>Represents the identifier of a selected Offer activity snapshot.</summary>
    public sealed record OfferActivityCreatedResponse(Guid Id);
}
