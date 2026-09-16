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

    [Fact]
    public void Requirement_sections_and_products_normalize_details_and_preserve_their_codes()
    {
        var activity = Activity.Create("Inspection", null, null, 0);
        var section = activity.AddRequirementSection(
            "  Exterior   works ", 12.5m, ActivityMeasurementUnit.SquareMeter, 0);
        var requirement = section.AddProductRequirement(
            CreateProduct(), 2.25m, false, ProductQuantityBehavior.Proportional,
            "  Use   weather-resistant   fixings ", 0);

        Assert.Equal("Exterior works", section.Name);
        Assert.Equal("m2", section.MeasurementUnit.ToCode());
        Assert.Equal("proportional", requirement.QuantityBehavior.ToCode());
        Assert.Equal("Use   weather-resistant   fixings", requirement.Notes);
        Assert.True(ActivityMeasurementUnitCodes.TryParse(" M3 ", out var unit));
        Assert.Equal(ActivityMeasurementUnit.CubicMeter, unit);
        Assert.True(ProductQuantityBehaviorCodes.TryParse(" FIXED ", out var behavior));
        Assert.Equal(ProductQuantityBehavior.Fixed, behavior);
    }

    [Fact]
    public void Requirement_entities_reject_invalid_values_duplicates_foreign_ownership_and_archived_changes()
    {
        var activity = Activity.Create("Inspection", null, null, 0);
        var section = activity.AddRequirementSection(null, 1m, ActivityMeasurementUnit.Piece, 0);
        var product = CreateProduct();
        section.AddProductRequirement(product, 1m, true, ProductQuantityBehavior.Fixed, null, 0);

        Assert.Throws<ArgumentOutOfRangeException>(() => section.UpdateDetails(null, 0m, ActivityMeasurementUnit.Piece));
        Assert.Throws<ArgumentException>(() => section.UpdateDetails(null, 1.00001m, ActivityMeasurementUnit.Piece));
        Assert.Throws<ArgumentOutOfRangeException>(() => section.UpdateDetails(null, 1m, (ActivityMeasurementUnit)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => section.UpdateDetails(new string('x', ActivityRequirementSection.MaxNameLength + 1), 1m, ActivityMeasurementUnit.Piece));
        Assert.Throws<InvalidOperationException>(() => section.AddProductRequirement(product, 1m, true, ProductQuantityBehavior.Fixed, null, 1));
        Assert.Throws<InvalidOperationException>(() => section.AddProductRequirement(CreateProduct(ProductStatus.Unavailable), 1m, true, ProductQuantityBehavior.Fixed, null, 1));
        Assert.Throws<ArgumentException>(() => section.AddProductRequirement(CreateProduct(), 1.00001m, true, ProductQuantityBehavior.Fixed, null, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => section.AddProductRequirement(CreateProduct(), 1m, true, (ProductQuantityBehavior)99, null, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => section.AddProductRequirement(CreateProduct(), 1m, true, ProductQuantityBehavior.Fixed, new string('x', ActivityProductRequirement.MaxNotesLength + 1), 1));

        var otherActivity = Activity.Create("Other", null, null, 0);
        var otherSection = otherActivity.AddRequirementSection(null, 1m, ActivityMeasurementUnit.Piece, 0);
        Assert.Throws<InvalidOperationException>(() => activity.RemoveRequirementSection(otherSection));

        activity.Archive();
        Assert.Throws<InvalidOperationException>(() => activity.AddRequirementSection(null, 1m, ActivityMeasurementUnit.Piece, 1));
        Assert.Throws<InvalidOperationException>(() => activity.RemoveRequirementSection(section));
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

    [Fact]
    public async Task Requirement_command_handlers_delegate_typed_values_to_the_requirement_service()
    {
        var service = Substitute.For<IActivityRequirementService>();
        var activityId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var sectionId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var productId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var requirementId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        service.CreateSectionAsync(activityId, "Exterior", 10m, ActivityMeasurementUnit.Meter, CancellationToken.None).Returns(sectionId);
        service.CreateProductRequirementAsync(activityId, sectionId, productId, 2m, true, ProductQuantityBehavior.Fixed, "Notes", CancellationToken.None).Returns(requirementId);

        Assert.Equal(sectionId, await new CreateActivityRequirementSectionHandler(service).Handle(new CreateActivityRequirementSectionCommand { ActivityId = activityId, Name = "Exterior", BasisQuantity = 10m, MeasurementUnit = "m" }, CancellationToken.None));
        Assert.Equal(requirementId, await new CreateActivityProductRequirementHandler(service).Handle(new CreateActivityProductRequirementCommand { ActivityId = activityId, SectionId = sectionId, ProductId = productId, Quantity = 2m, IsRequired = true, QuantityBehavior = "fixed", Notes = "Notes" }, CancellationToken.None));
        await new UpdateActivityRequirementSectionHandler(service).Handle(new UpdateActivityRequirementSectionCommand { ActivityId = activityId, SectionId = sectionId, Name = null, BasisQuantity = 1m, MeasurementUnit = "piece" }, CancellationToken.None);
        await new MoveActivityRequirementSectionHandler(service).Handle(new MoveActivityRequirementSectionCommand { ActivityId = activityId, SectionId = sectionId, TargetIndex = 0 }, CancellationToken.None);
        await new DeleteActivityRequirementSectionHandler(service).Handle(new DeleteActivityRequirementSectionCommand(activityId, sectionId), CancellationToken.None);
        await new UpdateActivityProductRequirementHandler(service).Handle(new UpdateActivityProductRequirementCommand { ActivityId = activityId, SectionId = sectionId, RequirementId = requirementId, Quantity = 1m, IsRequired = false, QuantityBehavior = "proportional", Notes = null }, CancellationToken.None);
        await new MoveActivityProductRequirementHandler(service).Handle(new MoveActivityProductRequirementCommand { ActivityId = activityId, SectionId = sectionId, RequirementId = requirementId, TargetIndex = 0 }, CancellationToken.None);
        await new DeleteActivityProductRequirementHandler(service).Handle(new DeleteActivityProductRequirementCommand(activityId, sectionId, requirementId), CancellationToken.None);

        await service.Received(1).UpdateSectionAsync(activityId, sectionId, null, 1m, ActivityMeasurementUnit.Piece, CancellationToken.None);
        await service.Received(1).MoveSectionAsync(activityId, sectionId, 0, CancellationToken.None);
        await service.Received(1).DeleteSectionAsync(activityId, sectionId, CancellationToken.None);
        await service.Received(1).UpdateProductRequirementAsync(activityId, sectionId, requirementId, 1m, false, ProductQuantityBehavior.Proportional, null, CancellationToken.None);
        await service.Received(1).MoveProductRequirementAsync(activityId, sectionId, requirementId, 0, CancellationToken.None);
        await service.Received(1).DeleteProductRequirementAsync(activityId, sectionId, requirementId, CancellationToken.None);
    }

    [Fact]
    public async Task Requirement_command_validators_reject_invalid_identifiers_quantities_and_codes()
    {
        var section = await new CreateActivityRequirementSectionValidator().ValidateAsync(new CreateActivityRequirementSectionCommand { ActivityId = Guid.Empty, Name = new string('x', ActivityRequirementSection.MaxNameLength + 1), BasisQuantity = 1.00001m, MeasurementUnit = "invalid" });
        var sectionUpdate = await new UpdateActivityRequirementSectionValidator().ValidateAsync(new UpdateActivityRequirementSectionCommand { ActivityId = Guid.Empty, SectionId = Guid.Empty, BasisQuantity = 0m, MeasurementUnit = "invalid" });
        var sectionMove = await new MoveActivityRequirementSectionValidator().ValidateAsync(new MoveActivityRequirementSectionCommand { ActivityId = Guid.Empty, SectionId = Guid.Empty, TargetIndex = -1 });
        var sectionDelete = await new DeleteActivityRequirementSectionValidator().ValidateAsync(new DeleteActivityRequirementSectionCommand(Guid.Empty, Guid.Empty));
        var product = await new CreateActivityProductRequirementValidator().ValidateAsync(new CreateActivityProductRequirementCommand { ActivityId = Guid.Empty, SectionId = Guid.Empty, ProductId = Guid.Empty, Quantity = 0m, QuantityBehavior = "invalid", Notes = new string('x', ActivityProductRequirement.MaxNotesLength + 1) });
        var productUpdate = await new UpdateActivityProductRequirementValidator().ValidateAsync(new UpdateActivityProductRequirementCommand { ActivityId = Guid.Empty, SectionId = Guid.Empty, RequirementId = Guid.Empty, Quantity = 0m, QuantityBehavior = "invalid" });
        var move = await new MoveActivityProductRequirementValidator().ValidateAsync(new MoveActivityProductRequirementCommand { ActivityId = Guid.NewGuid(), SectionId = Guid.NewGuid(), RequirementId = Guid.NewGuid(), TargetIndex = -1 });
        var productDelete = await new DeleteActivityProductRequirementValidator().ValidateAsync(new DeleteActivityProductRequirementCommand(Guid.Empty, Guid.Empty, Guid.Empty));

        Assert.False(section.IsValid);
        Assert.False(sectionUpdate.IsValid);
        Assert.False(sectionMove.IsValid);
        Assert.False(sectionDelete.IsValid);
        Assert.False(product.IsValid);
        Assert.False(productUpdate.IsValid);
        Assert.False(move.IsValid);
        Assert.False(productDelete.IsValid);
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
    [InlineData(typeof(CreateActivityRequirementSectionCommand))]
    [InlineData(typeof(UpdateActivityRequirementSectionCommand))]
    [InlineData(typeof(MoveActivityRequirementSectionCommand))]
    [InlineData(typeof(DeleteActivityRequirementSectionCommand))]
    [InlineData(typeof(CreateActivityProductRequirementCommand))]
    [InlineData(typeof(UpdateActivityProductRequirementCommand))]
    [InlineData(typeof(MoveActivityProductRequirementCommand))]
    [InlineData(typeof(DeleteActivityProductRequirementCommand))]
    public void Activity_catalog_use_cases_require_administrator_access(Type useCaseType)
    {
        var authorization = Assert.Single(useCaseType
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>());

        Assert.Equal(UserRoles.Administrator, authorization.Roles);
    }

    private static Product CreateProduct(ProductStatus status = ProductStatus.Active) => Product.Create(
        "Fixing", null, null, null, null, null, ProductCategory.Other,
        status, Domain.ValueObjects.ProductSearchConfiguration.Create(null));
}
