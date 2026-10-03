using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.RetailerPriceCollections;
using Application.SeedWork.Security;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public sealed class RetailerPriceCollectionRuns : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app
            .MapGroupCustom(customGroupName: "persons/{companyPersonId:guid}/price-collection-runs")
            .RequireAuthorization(AuthorizationPolicies.Administrator);

        group.MapPost(string.Empty, Start)
            .WithName("StartRetailerPriceCollectionRun")
            .WithSummary("Start durable automated price collection for a company")
            .Produces<RetailerPriceCollectionRunSummaryDto>(StatusCodes.Status202Accepted)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        group.MapGet(string.Empty, GetRecent)
            .WithName("GetRecentRetailerPriceCollectionRuns")
            .WithSummary("Get recent price-collection runs for a company")
            .Produces<IReadOnlyList<RetailerPriceCollectionRunSummaryDto>>()
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);
        group.MapGet("/{runId:guid}", GetById)
            .WithName("GetRetailerPriceCollectionRun")
            .WithSummary("Get a price-collection run and paged item results")
            .Produces<RetailerPriceCollectionRunDetailsDto>()
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<Accepted<RetailerPriceCollectionRunSummaryDto>> Start(
        IMediator mediator,
        Guid companyPersonId,
        CancellationToken cancellationToken)
    {
        var run = await mediator.Send(
            new StartRetailerPriceCollectionRunCommand(companyPersonId),
            cancellationToken);
        return TypedResults.Accepted(
            $"/persons/{companyPersonId}/price-collection-runs/{run.Id}",
            run);
    }

    private static async Task<Ok<IReadOnlyList<RetailerPriceCollectionRunSummaryDto>>> GetRecent(
        IMediator mediator,
        Guid companyPersonId,
        int limit = 10,
        CancellationToken cancellationToken = default) =>
        TypedResults.Ok(await mediator.Send(
            new RecentRetailerPriceCollectionRunsQuery(companyPersonId, limit),
            cancellationToken));

    private static async Task<Ok<RetailerPriceCollectionRunDetailsDto>> GetById(
        IMediator mediator,
        Guid companyPersonId,
        Guid runId,
        int pageIndex = 0,
        int pageSize = 25,
        CancellationToken cancellationToken = default) =>
        TypedResults.Ok(await mediator.Send(
            new RetailerPriceCollectionRunByIdQuery(
                companyPersonId,
                runId,
                pageIndex,
                pageSize),
            cancellationToken));
}
