using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.Assets;
using Application.SeedWork.Models;
using Application.SeedWork.Security;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public sealed class Assets : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroupCustom(customGroupName: "assets")
            .RequireAuthorization(AuthorizationPolicies.Administrator);
        group.MapGet(string.Empty, List).WithName("GetAssets").WithSummary("Get the filtered asset register").Produces<PagedResult<AssetListDto>>();
        group.MapGet("/{id:guid}", Get).WithName("GetAsset").WithSummary("Get tracked asset details").Produces<AssetDto>().Produces(StatusCodes.Status404NotFound);
        group.MapGet("/{id:guid}/events", Events).WithName("GetAssetEvents").WithSummary("Get chronological asset history").Produces<PagedResult<AssetEventDto>>();
        group.MapPost(string.Empty, Register).WithName("RegisterAssets").WithSummary("Atomically register one to fifty active assets").Produces<IReadOnlyList<RegisteredAssetDto>>(StatusCodes.Status201Created).ProducesValidationProblem();
        group.MapPut("/{id:guid}/details", UpdateDetails).WithName("UpdateAssetDetails").WithSummary("Update audited asset details").Produces<AssetDto>().ProducesValidationProblem().Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status409Conflict);
        MapAction<ReviewAssetRegistrationCommand>(group, "/{id:guid}/registration-review", "ReviewAssetRegistration", ReviewRegistration);
        MapAction<RelocateAssetCommand>(group, "/{id:guid}/site", "RelocateAsset", Relocate);
        MapAction<ChangeAssetConditionCommand>(group, "/{id:guid}/condition", "ChangeAssetCondition", ChangeCondition);
        MapAction<ChangeAssetOperationalStatusCommand>(group, "/{id:guid}/operational-status", "ChangeAssetOperationalStatus", ChangeStatus);
        MapAction<ReportAssetMissingCommand>(group, "/{id:guid}/missing", "ReportAssetMissing", Missing);
        MapAction<RetireAssetCommand>(group, "/{id:guid}/retirement", "RetireAsset", Retire);
        MapAction<ReassignAssetResponsibilityCommand>(group, "/{id:guid}/responsibility", "ReassignAssetResponsibility", Responsibility);
        MapAction<RotateAssetQrIdentifierCommand>(group, "/{id:guid}/qr-identifier", "RotateAssetQrIdentifier", RotateQr);
    }

    private static RouteHandlerBuilder MapAction<T>(
        RouteGroupBuilder group,
        string pattern,
        string name,
        Func<IMediator, Guid, T, CancellationToken, Task<Ok<AssetDto>>> handler)
        where T : AssetMutationRequest =>
        group.MapPatch(pattern, handler).WithName(name).WithSummary(name).Produces<AssetDto>().ProducesValidationProblem().Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status409Conflict);

    private static async Task<Ok<PagedResult<AssetListDto>>> List(IMediator mediator, [AsParameters] AssetsQuery query, CancellationToken ct) => TypedResults.Ok(await mediator.Send(query, ct));
    private static async Task<Ok<AssetDto>> Get(IMediator mediator, Guid id, CancellationToken ct) => TypedResults.Ok(await mediator.Send(new AssetByIdQuery(id), ct));
    private static async Task<Ok<PagedResult<AssetEventDto>>> Events(IMediator mediator, Guid id, int pageIndex = 0, int pageSize = 50, CancellationToken ct = default) => TypedResults.Ok(await mediator.Send(new AssetEventsQuery(id) { PageIndex = pageIndex, PageSize = pageSize }, ct));
    private static async Task<Created<IReadOnlyList<RegisteredAssetDto>>> Register(IMediator mediator, RegisterAssetsCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return TypedResults.Created($"/assets/{result[0].Id}", result);
    }
    private static Task<Ok<AssetDto>> ReviewRegistration(IMediator m, Guid id, ReviewAssetRegistrationCommand c, CancellationToken ct) => Send(m, c with { Id = id }, ct);
    private static Task<Ok<AssetDto>> UpdateDetails(IMediator m, Guid id, UpdateAssetDetailsCommand c, CancellationToken ct) => Send(m, c with { Id = id }, ct);
    private static Task<Ok<AssetDto>> Relocate(IMediator m, Guid id, RelocateAssetCommand c, CancellationToken ct) => Send(m, c with { Id = id }, ct);
    private static Task<Ok<AssetDto>> ChangeCondition(IMediator m, Guid id, ChangeAssetConditionCommand c, CancellationToken ct) => Send(m, c with { Id = id }, ct);
    private static Task<Ok<AssetDto>> ChangeStatus(IMediator m, Guid id, ChangeAssetOperationalStatusCommand c, CancellationToken ct) => Send(m, c with { Id = id }, ct);
    private static Task<Ok<AssetDto>> Missing(IMediator m, Guid id, ReportAssetMissingCommand c, CancellationToken ct) => Send(m, c with { Id = id }, ct);
    private static Task<Ok<AssetDto>> Retire(IMediator m, Guid id, RetireAssetCommand c, CancellationToken ct) => Send(m, c with { Id = id }, ct);
    private static Task<Ok<AssetDto>> Responsibility(IMediator m, Guid id, ReassignAssetResponsibilityCommand c, CancellationToken ct) => Send(m, c with { Id = id }, ct);
    private static Task<Ok<AssetDto>> RotateQr(IMediator m, Guid id, RotateAssetQrIdentifierCommand c, CancellationToken ct) => Send(m, c with { Id = id }, ct);
    private static async Task<Ok<AssetDto>> Send<T>(IMediator mediator, T command, CancellationToken ct) where T : IRequest<AssetDto> => TypedResults.Ok(await mediator.Send(command, ct));
}
