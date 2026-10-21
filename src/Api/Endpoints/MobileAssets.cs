using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.Assets;
using Application.SeedWork.Models;
using Application.SeedWork.Security;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public sealed class MobileAssets : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app
            .MapGroupCustom(customGroupName: "mobile/assets")
            .RequireAuthorization(AuthorizationPolicies.AdministratorOrWorker)
            .WithTags("Mobile Assets");

        group.MapPost("/discovery/qr", ByQr)
            .WithName("GetMobileAssetByQr")
            .WithSummary("Discover an accessible active asset by its current QR identifier")
            .Produces<MobileAssetDto>()
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
        group.MapGet("/search", Search)
            .WithName("SearchMobileAssets")
            .WithSummary("Browse or search accessible active assets")
            .Produces<PagedResult<MobileAssetListDto>>()
            .ProducesValidationProblem();
        group.MapGet("/sites", Sites)
            .WithName("GetMobileAssetSites")
            .WithSummary("Get Sites accessible to the authenticated user")
            .Produces<IReadOnlyList<MobileAssetSiteDto>>()
            .ProducesValidationProblem();
        group.MapGet("/{id:guid}", ById)
            .WithName("GetMobileAsset")
            .WithSummary("Get accessible active asset details")
            .Produces<MobileAssetDto>()
            .Produces(StatusCodes.Status404NotFound);
        group.MapPost("/registrations", Register)
            .WithName("RegisterMobileAsset")
            .WithSummary("Register one asset for administrator review")
            .Produces<MobileAssetRegistrationDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        group.MapGet("/registrations/{id:guid}", Registration)
            .WithName("GetMobileAssetRegistration")
            .WithSummary("Get the authenticated user's own asset registration status")
            .Produces<MobileAssetRegistrationDto>()
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<Ok<MobileAssetDto>> ByQr(
        IMediator mediator,
        MobileAssetByQrQuery query,
        CancellationToken ct) =>
        TypedResults.Ok(await mediator.Send(query, ct));

    private static async Task<Ok<PagedResult<MobileAssetListDto>>> Search(
        IMediator mediator,
        [AsParameters] MobileAssetsQuery query,
        CancellationToken ct) =>
        TypedResults.Ok(await mediator.Send(query, ct));

    private static async Task<Ok<IReadOnlyList<MobileAssetSiteDto>>> Sites(
        IMediator mediator,
        [AsParameters] MobileAssetSitesQuery query,
        CancellationToken ct) =>
        TypedResults.Ok(await mediator.Send(query, ct));

    private static async Task<Ok<MobileAssetDto>> ById(
        IMediator mediator,
        Guid id,
        CancellationToken ct) =>
        TypedResults.Ok(await mediator.Send(new MobileAssetByIdQuery(id), ct));

    private static async Task<Created<MobileAssetRegistrationDto>> Register(
        IMediator mediator,
        RegisterMobileAssetCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return TypedResults.Created($"/mobile/assets/registrations/{result.Id}", result);
    }

    private static async Task<Ok<MobileAssetRegistrationDto>> Registration(
        IMediator mediator,
        Guid id,
        CancellationToken ct) =>
        TypedResults.Ok(await mediator.Send(new MobileAssetRegistrationByIdQuery(id), ct));
}
