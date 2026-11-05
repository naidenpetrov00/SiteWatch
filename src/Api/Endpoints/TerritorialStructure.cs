using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.SeedWork.Security;
using Application.TerritorialStructure;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public sealed class TerritorialStructure : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroupCustom(customGroupName: "territorial-structure")
            .RequireAuthorization(AuthorizationPolicies.Administrator);

        group.MapGet(string.Empty, GetEntries).WithName("GetTerritorialEntries").WithSummary("Search territorial structure entries").Produces<IReadOnlyList<TerritorialEntryDto>>();
        group.MapPost(string.Empty, CreateEntry).WithName("CreateTerritorialEntry").WithSummary("Create a territorial structure entry").Produces<EntryCreatedResponse>(StatusCodes.Status201Created).ProducesValidationProblem().Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status409Conflict);
        group.MapPut("/{entryId:guid}", UpdateEntry).WithName("UpdateTerritorialEntry").WithSummary("Rename or change the active state of a territorial structure entry").Produces(StatusCodes.Status204NoContent).ProducesValidationProblem().Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status409Conflict);
        group.MapDelete("/{entryId:guid}", DeleteEntry).WithName("DeleteTerritorialEntry").WithSummary("Delete an unreferenced territorial structure entry").Produces(StatusCodes.Status204NoContent).ProducesValidationProblem().Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status409Conflict);
    }

    private static async Task<Ok<IReadOnlyList<TerritorialEntryDto>>> GetEntries(IMediator mediator, [AsParameters] TerritorialEntriesQuery query, CancellationToken cancellationToken) => TypedResults.Ok(await mediator.Send(query, cancellationToken));
    private static async Task<Created<EntryCreatedResponse>> CreateEntry(IMediator mediator, CreateTerritorialEntryCommand command, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/territorial-structure/{id}", new EntryCreatedResponse(id));
    }
    private static async Task<NoContent> UpdateEntry(IMediator mediator, Guid entryId, UpdateTerritorialEntryCommand command, CancellationToken cancellationToken)
    {
        command.Id = entryId;
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteEntry(IMediator mediator, Guid entryId, [AsParameters] TerritorialEntryDeleteRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteTerritorialEntryCommand(request.Kind, entryId), cancellationToken);
        return TypedResults.NoContent();
    }

    public sealed record EntryCreatedResponse(Guid Id);
    public sealed record TerritorialEntryDeleteRequest(TerritorialEntryKind Kind);
}
