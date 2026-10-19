using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.Assets;
using Application.SeedWork.Models;
using Application.SeedWork.Security;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public sealed class AssetCatalogItems : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroupCustom(customGroupName: "asset-catalog-items")
            .RequireAuthorization(AuthorizationPolicies.Administrator);

        group.MapGet(string.Empty, List).WithName("GetAssetCatalogItems").WithSummary("Get the filtered asset catalog").Produces<PagedResult<AssetCatalogItemListDto>>();
        group.MapGet("/search", Search).WithName("SearchAssetCatalogItems").WithSummary("Search active catalog items for assignment").Produces<List<AssetCatalogItemLookupDto>>();
        group.MapGet("/{id:guid}", Get).WithName("GetAssetCatalogItem").WithSummary("Get asset catalog details").Produces<AssetCatalogItemDto>().Produces(StatusCodes.Status404NotFound);
        group.MapPost(string.Empty, Create).WithName("CreateAssetCatalogItem").WithSummary("Create an active Machine or Tool catalog item").Produces<AssetCatalogItemDto>(StatusCodes.Status201Created).ProducesValidationProblem();
        group.MapPut("/{id:guid}", Update).WithName("UpdateAssetCatalogItem").WithSummary("Update asset catalog details").Produces<AssetCatalogItemDto>().ProducesValidationProblem().Produces(StatusCodes.Status409Conflict);
        group.MapPatch("/{id:guid}/review", Review).WithName("ReviewAssetCatalogItem").WithSummary("Accept or reject a pending catalog item").Produces<AssetCatalogItemDto>().ProducesValidationProblem().Produces(StatusCodes.Status409Conflict);
    }

    private static async Task<Ok<PagedResult<AssetCatalogItemListDto>>> List(IMediator mediator, [AsParameters] AssetCatalogItemsQuery query, CancellationToken ct) => TypedResults.Ok(await mediator.Send(query, ct));
    private static async Task<Ok<List<AssetCatalogItemLookupDto>>> Search(IMediator mediator, [AsParameters] AssetCatalogItemSearchQuery query, CancellationToken ct) => TypedResults.Ok(await mediator.Send(query, ct));
    private static async Task<Ok<AssetCatalogItemDto>> Get(IMediator mediator, Guid id, CancellationToken ct) => TypedResults.Ok(await mediator.Send(new AssetCatalogItemByIdQuery(id), ct));
    private static async Task<Created<AssetCatalogItemDto>> Create(IMediator mediator, CreateAssetCatalogItemCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return TypedResults.Created($"/asset-catalog-items/{result.Id}", result);
    }
    private static async Task<Ok<AssetCatalogItemDto>> Update(IMediator mediator, Guid id, UpdateAssetCatalogItemCommand command, CancellationToken ct) => TypedResults.Ok(await mediator.Send(command with { Id = id }, ct));
    private static async Task<Ok<AssetCatalogItemDto>> Review(IMediator mediator, Guid id, ReviewAssetCatalogItemCommand command, CancellationToken ct) => TypedResults.Ok(await mediator.Send(command with { Id = id }, ct));
}
