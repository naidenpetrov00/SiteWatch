using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.Offers.Defaults;
using Application.SeedWork.Security;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public sealed class OfferDefaultRetailers : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app
            .MapGroupCustom(customGroupName: "dashboard/offer-default-retailers")
            .RequireAuthorization(AuthorizationPolicies.Administrator);

        group.MapGet(string.Empty, GetDefaults)
            .WithName("GetDefaultOfferRetailers")
            .WithSummary("Get the ordered global retailer defaults for new Offers")
            .Produces<IReadOnlyList<DefaultOfferRetailerDto>>(StatusCodes.Status200OK);
        group.MapPut(string.Empty, ReplaceDefaults)
            .WithName("ReplaceDefaultOfferRetailers")
            .WithSummary("Replace the ordered global retailer defaults for new Offers")
            .Produces<IReadOnlyList<DefaultOfferRetailerDto>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status409Conflict);
    }

    private static async Task<Ok<IReadOnlyList<DefaultOfferRetailerDto>>> GetDefaults(
        IMediator mediator,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Send(
            new DefaultOfferRetailersQuery(),
            cancellationToken));

    private static async Task<Ok<IReadOnlyList<DefaultOfferRetailerDto>>> ReplaceDefaults(
        IMediator mediator,
        ReplaceDefaultOfferRetailersCommand command,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Send(command, cancellationToken));
}
