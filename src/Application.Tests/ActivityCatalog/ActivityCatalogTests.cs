using Application.ActivityCatalog.Commands;
using Application.ActivityCatalog.Queries;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Domain.Entities;
using Domain.SeedWork.Enums;
using NSubstitute;

namespace Application.Tests.ActivityCatalog;

public sealed class ActivityCatalogTests
{
    [Fact]
    public void Catalog_nodes_normalize_names_and_activity_details()
    {
        var activity = Activity.Create("  Site   inspection ", "  Check   the gate  ", null, 0);

        Assert.Equal("Site inspection", activity.Name);
        Assert.Equal("SITE INSPECTION", activity.NormalizedName);
        Assert.Equal("Check   the gate", activity.Description);

        activity.UpdateDetails("  Daily check ", "   ");

        Assert.Equal("Daily check", activity.Name);
        Assert.Equal("DAILY CHECK", activity.NormalizedName);
        Assert.Null(activity.Description);
    }

    [Fact]
    public void Catalog_nodes_enforce_name_sort_order_and_parent_boundaries()
    {
        Assert.Throws<ArgumentException>(() => ActivityFolder.Create(" ", null, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => ActivityFolder.Create("Folder", null, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ActivityFolder.Create(new string('x', ActivityCatalogNode.MaxNameLength + 1), null, 0));

        var folder = ActivityFolder.Create("Folder", null, 0);
        Assert.Throws<InvalidOperationException>(() => folder.MoveTo(folder, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => folder.SetSortOrder(-1));
    }

    [Fact]
    public void Activities_enforce_description_length_and_transition_status()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Activity.Create(
            "Inspection", new string('x', Activity.MaxDescriptionLength + 1), null, 0));

        var activity = Activity.Create("Inspection", null, null, 0);
        Assert.Equal(ActivityStatus.Active, activity.Status);

        activity.Archive();
        Assert.Equal(ActivityStatus.Archived, activity.Status);

        activity.Restore();
        Assert.Equal(ActivityStatus.Active, activity.Status);
    }

    [Theory]
    [InlineData("", true, 0)]
    [InlineData("Inspection", false, -1)]
    [InlineData("Inspection", false, 0)]
    public async Task Activity_command_validators_enforce_required_names_parent_ids_and_positions(
        string name,
        bool useEmptyParent,
        int targetIndex)
    {
        var create = await new CreateActivityValidator().ValidateAsync(new CreateActivityCommand(
            name, null, useEmptyParent ? Guid.Empty : null));
        var move = await new MoveActivityValidator().ValidateAsync(new MoveActivityCommand
        {
            Id = Guid.NewGuid(),
            TargetParentFolderId = useEmptyParent ? Guid.Empty : null,
            TargetIndex = targetIndex,
        });

        Assert.Equal(name != "" && !useEmptyParent, create.IsValid);
        Assert.Equal(!useEmptyParent && targetIndex >= 0, move.IsValid);
    }

    [Fact]
    public async Task Folder_and_activity_validators_reject_missing_identifiers_and_oversized_values()
    {
        var folder = await new RenameActivityFolderValidator().ValidateAsync(
            new RenameActivityFolderCommand { Name = new string('x', ActivityCatalogNode.MaxNameLength + 1) });
        var activity = await new UpdateActivityValidator().ValidateAsync(
            new UpdateActivityCommand { Name = "Activity", Description = new string('x', Activity.MaxDescriptionLength + 1) });
        var details = await new ActivityDetailsQueryValidator().ValidateAsync(new ActivityDetailsQuery(Guid.Empty));

        Assert.Contains(folder.Errors, error => error.PropertyName == nameof(RenameActivityFolderCommand.Id));
        Assert.Contains(folder.Errors, error => error.PropertyName == nameof(RenameActivityFolderCommand.Name));
        Assert.Contains(activity.Errors, error => error.PropertyName == nameof(UpdateActivityCommand.Id));
        Assert.Contains(activity.Errors, error => error.PropertyName == nameof(UpdateActivityCommand.Description));
        Assert.Contains(details.Errors, error => error.PropertyName == nameof(ActivityDetailsQuery.ActivityId));
    }

    [Fact]
    public async Task Activity_catalog_command_handlers_delegate_their_contracts_to_the_service()
    {
        var service = Substitute.For<IActivityCatalogService>();
        var folderId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var activityId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var parentId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        service.CreateFolderAsync("Folder", parentId, CancellationToken.None).Returns(folderId);
        service.CreateActivityAsync("Activity", "Description", parentId, CancellationToken.None).Returns(activityId);

        var createdFolder = await new CreateActivityFolderHandler(service).Handle(
            new CreateActivityFolderCommand("Folder", parentId), CancellationToken.None);
        var createdActivity = await new CreateActivityHandler(service).Handle(
            new CreateActivityCommand("Activity", "Description", parentId), CancellationToken.None);
        await new RenameActivityFolderHandler(service).Handle(
            new RenameActivityFolderCommand { Id = folderId, Name = "Renamed" }, CancellationToken.None);
        await new MoveActivityFolderHandler(service).Handle(
            new MoveActivityFolderCommand { Id = folderId, TargetParentFolderId = null, TargetIndex = 0 }, CancellationToken.None);
        await new DeleteActivityFolderHandler(service).Handle(new DeleteActivityFolderCommand(folderId), CancellationToken.None);
        await new UpdateActivityHandler(service).Handle(
            new UpdateActivityCommand { Id = activityId, Name = "Updated", Description = null }, CancellationToken.None);
        await new MoveActivityHandler(service).Handle(
            new MoveActivityCommand { Id = activityId, TargetParentFolderId = null, TargetIndex = 0 }, CancellationToken.None);
        await new SetActivityArchivedHandler(service).Handle(new SetActivityArchivedCommand(activityId, true), CancellationToken.None);
        await new DeleteActivityHandler(service).Handle(new DeleteActivityCommand(activityId), CancellationToken.None);

        Assert.Equal(folderId, createdFolder);
        Assert.Equal(activityId, createdActivity);
        await service.Received(1).RenameFolderAsync(folderId, "Renamed", CancellationToken.None);
        await service.Received(1).MoveFolderAsync(folderId, null, 0, CancellationToken.None);
        await service.Received(1).DeleteFolderAsync(folderId, CancellationToken.None);
        await service.Received(1).UpdateActivityAsync(activityId, "Updated", null, CancellationToken.None);
        await service.Received(1).MoveActivityAsync(activityId, null, 0, CancellationToken.None);
        await service.Received(1).SetActivityArchivedAsync(activityId, true, CancellationToken.None);
        await service.Received(1).DeleteActivityAsync(activityId, CancellationToken.None);
    }

    [Theory]
    [InlineData(typeof(CreateActivityFolderCommand))]
    [InlineData(typeof(RenameActivityFolderCommand))]
    [InlineData(typeof(MoveActivityFolderCommand))]
    [InlineData(typeof(DeleteActivityFolderCommand))]
    [InlineData(typeof(CreateActivityCommand))]
    [InlineData(typeof(UpdateActivityCommand))]
    [InlineData(typeof(MoveActivityCommand))]
    [InlineData(typeof(SetActivityArchivedCommand))]
    [InlineData(typeof(DeleteActivityCommand))]
    [InlineData(typeof(ActivityCatalogTreeQuery))]
    [InlineData(typeof(ActivityDetailsQuery))]
    public void Activity_catalog_use_cases_require_administrator_access(Type useCaseType)
    {
        var authorization = Assert.Single(useCaseType
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>());

        Assert.Equal(UserRoles.Administrator, authorization.Roles);
    }
}
