using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.ActivityCatalog.Commands;
using Application.ActivityCatalog.Queries;
using Application.SeedWork.Security;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public sealed class ActivityCatalog : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var folderGroup = app
            .MapGroupCustom(customGroupName: "activity-folders")
            .RequireAuthorization(AuthorizationPolicies.Administrator);
        var activityGroup = app
            .MapGroupCustom(customGroupName: "activities")
            .RequireAuthorization(AuthorizationPolicies.Administrator);
        var dashboardGroup = app
            .MapGroupCustom(customGroupName: "dashboard")
            .RequireAuthorization(AuthorizationPolicies.Administrator);

        dashboardGroup.MapGet("/activity-catalog", GetTree)
            .WithName("GetActivityCatalog")
            .WithSummary("Get the complete activity catalog tree");

        folderGroup.MapPost(string.Empty, CreateFolder)
            .WithName("CreateActivityFolder")
            .WithSummary("Create a root or nested activity folder");
        folderGroup.MapPut("/{folderId:guid}", RenameFolder)
            .WithName("RenameActivityFolder")
            .WithSummary("Rename an activity folder");
        folderGroup.MapPatch("/{folderId:guid}/move", MoveFolder)
            .WithName("MoveActivityFolder")
            .WithSummary("Move or reorder an activity folder");
        folderGroup.MapDelete("/{folderId:guid}", DeleteFolder)
            .WithName("DeleteActivityFolder")
            .WithSummary("Delete an empty activity folder");

        activityGroup.MapPost(string.Empty, CreateActivity)
            .WithName("CreateActivity")
            .WithSummary("Create a root or nested activity");
        activityGroup.MapGet("/{activityId:guid}", GetActivity)
            .WithName("GetActivity")
            .WithSummary("Get an activity by ID");
        activityGroup.MapPut("/{activityId:guid}", UpdateActivity)
            .WithName("UpdateActivity")
            .WithSummary("Update an activity");
        activityGroup.MapPatch("/{activityId:guid}/move", MoveActivity)
            .WithName("MoveActivity")
            .WithSummary("Move or reorder an activity");
        activityGroup.MapPatch("/{activityId:guid}/archive", ArchiveActivity)
            .WithName("ArchiveActivity")
            .WithSummary("Archive an activity");
        activityGroup.MapPatch("/{activityId:guid}/restore", RestoreActivity)
            .WithName("RestoreActivity")
            .WithSummary("Restore an archived activity");
        activityGroup.MapDelete("/{activityId:guid}", DeleteActivity)
            .WithName("DeleteActivity")
            .WithSummary("Delete an unreferenced activity");
    }

    private static async Task<Ok<List<ActivityCatalogNodeDto>>> GetTree(
        IMediator mediator,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Send(new ActivityCatalogTreeQuery(), cancellationToken));

    private static async Task<IResult> CreateFolder(
        IMediator mediator,
        CreateActivityFolderCommand command,
        CancellationToken cancellationToken)
    {
        var folderId = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/activity-folders/{folderId}", new { id = folderId });
    }

    private static async Task<NoContent> RenameFolder(
        IMediator mediator,
        Guid folderId,
        RenameActivityFolderCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = folderId;
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> MoveFolder(
        IMediator mediator,
        Guid folderId,
        MoveActivityFolderCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = folderId;
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteFolder(
        IMediator mediator,
        Guid folderId,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteActivityFolderCommand(folderId), cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<IResult> CreateActivity(
        IMediator mediator,
        CreateActivityCommand command,
        CancellationToken cancellationToken)
    {
        var activityId = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/activities/{activityId}", new { id = activityId });
    }

    private static async Task<Ok<ActivityDetailsDto>> GetActivity(
        IMediator mediator,
        Guid activityId,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Send(
            new ActivityDetailsQuery(activityId),
            cancellationToken));

    private static async Task<NoContent> UpdateActivity(
        IMediator mediator,
        Guid activityId,
        UpdateActivityCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = activityId;
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> MoveActivity(
        IMediator mediator,
        Guid activityId,
        MoveActivityCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = activityId;
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static Task<NoContent> ArchiveActivity(
        IMediator mediator,
        Guid activityId,
        CancellationToken cancellationToken) =>
        SetArchived(mediator, activityId, true, cancellationToken);

    private static Task<NoContent> RestoreActivity(
        IMediator mediator,
        Guid activityId,
        CancellationToken cancellationToken) =>
        SetArchived(mediator, activityId, false, cancellationToken);

    private static async Task<NoContent> SetArchived(
        IMediator mediator,
        Guid activityId,
        bool archived,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new SetActivityArchivedCommand(activityId, archived),
            cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteActivity(
        IMediator mediator,
        Guid activityId,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteActivityCommand(activityId), cancellationToken);
        return TypedResults.NoContent();
    }
}
