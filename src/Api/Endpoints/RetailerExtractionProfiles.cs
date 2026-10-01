using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.RetailerExtractionProfiles;
using Application.SeedWork.Security;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public sealed class RetailerExtractionProfiles : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app
            .MapGroupCustom(
                customGroupName: "retailers/{retailerId:guid}/price-extraction-profiles")
            .RequireAuthorization(AuthorizationPolicies.Administrator);

        group.MapGet(string.Empty, GetVersions)
            .WithName("GetRetailerExtractionProfileVersions")
            .WithSummary("Get all extraction-profile versions for a retailer")
            .Produces<IReadOnlyList<RetailerExtractionProfileSummaryDto>>()
            .Produces(StatusCodes.Status404NotFound);
        group.MapGet("/current", GetCurrent)
            .WithName("GetCurrentRetailerExtractionProfiles")
            .WithSummary("Get the current draft and active extraction profile")
            .Produces<RetailerExtractionCurrentProfilesDto>()
            .Produces(StatusCodes.Status404NotFound);
        group.MapGet("/{profileId:guid}", GetProfile)
            .WithName("GetRetailerExtractionProfile")
            .WithSummary("Get one complete extraction-profile version")
            .Produces<RetailerExtractionProfileDetailsDto>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        group.MapPost("/drafts", CreateDraft)
            .WithName("CreateRetailerExtractionDraft")
            .WithSummary("Create the first draft or clone a published version")
            .Produces<RetailerExtractionProfileDetailsDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        group.MapDelete("/{profileId:guid}", DeleteDraft)
            .WithName("DeleteRetailerExtractionDraft")
            .WithSummary("Permanently discard a draft extraction profile")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        group.MapPut("/{profileId:guid}/allowed-hosts", UpdateAllowedHosts)
            .WithName("UpdateRetailerExtractionAllowedHosts")
            .WithSummary("Replace the allowed HTTPS hosts for a draft")
            .Produces<RetailerExtractionProfileDetailsDto>()
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        group.MapPost("/{profileId:guid}/rules", AddRule)
            .WithName("AddRetailerExtractionRule")
            .WithSummary("Append an extraction rule to a draft")
            .Produces<RetailerExtractionProfileDetailsDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        group.MapPut("/{profileId:guid}/rules/{ruleId:guid}", UpdateRule)
            .WithName("UpdateRetailerExtractionRule")
            .WithSummary("Update one extraction rule in a draft")
            .Produces<RetailerExtractionProfileDetailsDto>()
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        group.MapDelete("/{profileId:guid}/rules/{ruleId:guid}", DeleteRule)
            .WithName("DeleteRetailerExtractionRule")
            .WithSummary("Delete one extraction rule from a draft")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        group.MapPatch("/{profileId:guid}/rules/{ruleId:guid}/enable", EnableRule)
            .WithName("EnableRetailerExtractionRule")
            .WithSummary("Enable one extraction rule in a draft")
            .Produces<RetailerExtractionProfileDetailsDto>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        group.MapPatch("/{profileId:guid}/rules/{ruleId:guid}/disable", DisableRule)
            .WithName("DisableRetailerExtractionRule")
            .WithSummary("Disable one extraction rule in a draft")
            .Produces<RetailerExtractionProfileDetailsDto>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        group.MapPut("/{profileId:guid}/rules/order", ReorderRules)
            .WithName("ReorderRetailerExtractionRules")
            .WithSummary("Replace the complete rule order for a draft")
            .Produces<RetailerExtractionProfileDetailsDto>()
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        group.MapPost("/{profileId:guid}/test", Test)
            .WithName("TestRetailerExtractionProfile")
            .WithSummary("Test the saved draft extraction profile against one product URL")
            .Produces<RetailerExtractionTestResultDto>()
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status422UnprocessableEntity)
            .Produces(StatusCodes.Status502BadGateway)
            .Produces(StatusCodes.Status504GatewayTimeout);
        group.MapPatch("/{profileId:guid}/publish", Publish)
            .WithName("PublishRetailerExtractionProfile")
            .WithSummary("Publish and activate a complete draft")
            .Produces<RetailerExtractionProfileDetailsDto>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        group.MapPatch("/{profileId:guid}/activate", Activate)
            .WithName("ActivateRetailerExtractionProfile")
            .WithSummary("Activate a retained published profile version")
            .Produces<RetailerExtractionProfileDetailsDto>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
    }

    private static async Task<Ok<IReadOnlyList<RetailerExtractionProfileSummaryDto>>> GetVersions(
        IMediator mediator,
        Guid retailerId,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Send(
            new RetailerExtractionProfileVersionsQuery(retailerId),
            cancellationToken));

    private static async Task<Ok<RetailerExtractionCurrentProfilesDto>> GetCurrent(
        IMediator mediator,
        Guid retailerId,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Send(
            new RetailerExtractionCurrentProfilesQuery(retailerId),
            cancellationToken));

    private static async Task<Ok<RetailerExtractionProfileDetailsDto>> GetProfile(
        IMediator mediator,
        Guid retailerId,
        Guid profileId,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Send(
            new RetailerExtractionProfileByIdQuery(retailerId, profileId),
            cancellationToken));

    private static async Task<Created<RetailerExtractionProfileDetailsDto>> CreateDraft(
        IMediator mediator,
        Guid retailerId,
        CreateRetailerExtractionDraftCommand command,
        CancellationToken cancellationToken)
    {
        command.RetailerId = retailerId;
        var profile = await mediator.Send(command, cancellationToken);
        return TypedResults.Created(
            $"/retailers/{retailerId}/price-extraction-profiles/{profile.Summary.Id}",
            profile);
    }

    private static async Task<NoContent> DeleteDraft(
        IMediator mediator,
        Guid retailerId,
        Guid profileId,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new DeleteRetailerExtractionDraftCommand(retailerId, profileId),
            cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<RetailerExtractionProfileDetailsDto>> UpdateAllowedHosts(
        IMediator mediator,
        Guid retailerId,
        Guid profileId,
        UpdateRetailerExtractionAllowedHostsCommand command,
        CancellationToken cancellationToken)
    {
        command.RetailerId = retailerId;
        command.ProfileId = profileId;
        return TypedResults.Ok(await mediator.Send(command, cancellationToken));
    }

    private static async Task<Created<RetailerExtractionProfileDetailsDto>> AddRule(
        IMediator mediator,
        Guid retailerId,
        Guid profileId,
        AddRetailerExtractionRuleCommand command,
        CancellationToken cancellationToken)
    {
        command.RetailerId = retailerId;
        command.ProfileId = profileId;
        var profile = await mediator.Send(command, cancellationToken);
        return TypedResults.Created(
            $"/retailers/{retailerId}/price-extraction-profiles/{profileId}",
            profile);
    }

    private static async Task<Ok<RetailerExtractionProfileDetailsDto>> UpdateRule(
        IMediator mediator,
        Guid retailerId,
        Guid profileId,
        Guid ruleId,
        UpdateRetailerExtractionRuleCommand command,
        CancellationToken cancellationToken)
    {
        command.RetailerId = retailerId;
        command.ProfileId = profileId;
        command.RuleId = ruleId;
        return TypedResults.Ok(await mediator.Send(command, cancellationToken));
    }

    private static async Task<NoContent> DeleteRule(
        IMediator mediator,
        Guid retailerId,
        Guid profileId,
        Guid ruleId,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new DeleteRetailerExtractionRuleCommand(retailerId, profileId, ruleId),
            cancellationToken);
        return TypedResults.NoContent();
    }

    private static Task<Ok<RetailerExtractionProfileDetailsDto>> EnableRule(
        IMediator mediator,
        Guid retailerId,
        Guid profileId,
        Guid ruleId,
        CancellationToken cancellationToken) =>
        SetRuleEnabled(mediator, retailerId, profileId, ruleId, true, cancellationToken);

    private static Task<Ok<RetailerExtractionProfileDetailsDto>> DisableRule(
        IMediator mediator,
        Guid retailerId,
        Guid profileId,
        Guid ruleId,
        CancellationToken cancellationToken) =>
        SetRuleEnabled(mediator, retailerId, profileId, ruleId, false, cancellationToken);

    private static async Task<Ok<RetailerExtractionProfileDetailsDto>> SetRuleEnabled(
        IMediator mediator,
        Guid retailerId,
        Guid profileId,
        Guid ruleId,
        bool isEnabled,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Send(
            new SetRetailerExtractionRuleEnabledCommand(
                retailerId,
                profileId,
                ruleId,
                isEnabled),
            cancellationToken));

    private static async Task<Ok<RetailerExtractionProfileDetailsDto>> ReorderRules(
        IMediator mediator,
        Guid retailerId,
        Guid profileId,
        ReorderRetailerExtractionRulesCommand command,
        CancellationToken cancellationToken)
    {
        command.RetailerId = retailerId;
        command.ProfileId = profileId;
        return TypedResults.Ok(await mediator.Send(command, cancellationToken));
    }

    private static async Task<Ok<RetailerExtractionProfileDetailsDto>> Publish(
        IMediator mediator,
        Guid retailerId,
        Guid profileId,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Send(
            new PublishRetailerExtractionProfileCommand(retailerId, profileId),
            cancellationToken));

    private static async Task<Ok<RetailerExtractionTestResultDto>> Test(
        IMediator mediator,
        Guid retailerId,
        Guid profileId,
        TestRetailerExtractionProfileCommand command,
        CancellationToken cancellationToken)
    {
        command.RetailerId = retailerId;
        command.ProfileId = profileId;
        return TypedResults.Ok(await mediator.Send(command, cancellationToken));
    }

    private static async Task<Ok<RetailerExtractionProfileDetailsDto>> Activate(
        IMediator mediator,
        Guid retailerId,
        Guid profileId,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Send(
            new ActivateRetailerExtractionProfileCommand(retailerId, profileId),
            cancellationToken));
}
