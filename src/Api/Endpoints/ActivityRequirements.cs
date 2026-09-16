using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.ActivityCatalog.Commands;
using Application.SeedWork.Security;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public sealed class ActivityRequirements : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var activityGroup = app
            .MapGroupCustom(customGroupName: "activities")
            .RequireAuthorization(AuthorizationPolicies.Administrator);

        activityGroup.MapPost(
                "/{activityId:guid}/requirement-sections",
                CreateRequirementSection)
            .WithName("CreateActivityRequirementSection")
            .WithSummary("Add a measurement-based product requirement section");
        activityGroup.MapPut(
                "/{activityId:guid}/requirement-sections/{sectionId:guid}",
                UpdateRequirementSection)
            .WithName("UpdateActivityRequirementSection")
            .WithSummary("Update an activity requirement section");
        activityGroup.MapPatch(
                "/{activityId:guid}/requirement-sections/{sectionId:guid}/move",
                MoveRequirementSection)
            .WithName("MoveActivityRequirementSection")
            .WithSummary("Reorder an activity requirement section");
        activityGroup.MapDelete(
                "/{activityId:guid}/requirement-sections/{sectionId:guid}",
                DeleteRequirementSection)
            .WithName("DeleteActivityRequirementSection")
            .WithSummary("Delete an activity requirement section and its relationships");
        activityGroup.MapPost(
                "/{activityId:guid}/requirement-sections/{sectionId:guid}/products",
                CreateProductRequirement)
            .WithName("CreateActivityProductRequirement")
            .WithSummary("Assign an active Product Catalog item to a requirement section");
        activityGroup.MapPut(
                "/{activityId:guid}/requirement-sections/{sectionId:guid}/products/{requirementId:guid}",
                UpdateProductRequirement)
            .WithName("UpdateActivityProductRequirement")
            .WithSummary("Update an activity product requirement");
        activityGroup.MapPatch(
                "/{activityId:guid}/requirement-sections/{sectionId:guid}/products/{requirementId:guid}/move",
                MoveProductRequirement)
            .WithName("MoveActivityProductRequirement")
            .WithSummary("Reorder a product requirement within its section");
        activityGroup.MapDelete(
                "/{activityId:guid}/requirement-sections/{sectionId:guid}/products/{requirementId:guid}",
                DeleteProductRequirement)
            .WithName("DeleteActivityProductRequirement")
            .WithSummary("Remove a Product Catalog relationship from a requirement section");
    }

    private static async Task<IResult> CreateRequirementSection(
        IMediator mediator,
        Guid activityId,
        CreateActivityRequirementSectionCommand command,
        CancellationToken cancellationToken)
    {
        command.ActivityId = activityId;
        var sectionId = await mediator.Send(command, cancellationToken);
        return TypedResults.Created(
            $"/activities/{activityId}/requirement-sections/{sectionId}",
            new { id = sectionId });
    }

    private static async Task<NoContent> UpdateRequirementSection(
        IMediator mediator,
        Guid activityId,
        Guid sectionId,
        UpdateActivityRequirementSectionCommand command,
        CancellationToken cancellationToken)
    {
        command.ActivityId = activityId;
        command.SectionId = sectionId;
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> MoveRequirementSection(
        IMediator mediator,
        Guid activityId,
        Guid sectionId,
        MoveActivityRequirementSectionCommand command,
        CancellationToken cancellationToken)
    {
        command.ActivityId = activityId;
        command.SectionId = sectionId;
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteRequirementSection(
        IMediator mediator,
        Guid activityId,
        Guid sectionId,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new DeleteActivityRequirementSectionCommand(activityId, sectionId),
            cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<IResult> CreateProductRequirement(
        IMediator mediator,
        Guid activityId,
        Guid sectionId,
        CreateActivityProductRequirementCommand command,
        CancellationToken cancellationToken)
    {
        command.ActivityId = activityId;
        command.SectionId = sectionId;
        var requirementId = await mediator.Send(command, cancellationToken);
        return TypedResults.Created(
            $"/activities/{activityId}/requirement-sections/{sectionId}/products/{requirementId}",
            new { id = requirementId });
    }

    private static async Task<NoContent> UpdateProductRequirement(
        IMediator mediator,
        Guid activityId,
        Guid sectionId,
        Guid requirementId,
        UpdateActivityProductRequirementCommand command,
        CancellationToken cancellationToken)
    {
        command.ActivityId = activityId;
        command.SectionId = sectionId;
        command.RequirementId = requirementId;
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> MoveProductRequirement(
        IMediator mediator,
        Guid activityId,
        Guid sectionId,
        Guid requirementId,
        MoveActivityProductRequirementCommand command,
        CancellationToken cancellationToken)
    {
        command.ActivityId = activityId;
        command.SectionId = sectionId;
        command.RequirementId = requirementId;
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteProductRequirement(
        IMediator mediator,
        Guid activityId,
        Guid sectionId,
        Guid requirementId,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new DeleteActivityProductRequirementCommand(
                activityId,
                sectionId,
                requirementId),
            cancellationToken);
        return TypedResults.NoContent();
    }
}
