using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Tests.Infrastructure;
using Application.ActivityCatalog.Commands;
using Application.ActivityCatalog.Queries;
using NSubstitute;

namespace Api.Tests.Endpoints.ActivityCatalog;

public sealed class ActivityCatalogEndpointsTests
{
    [Fact]
    public async Task Catalog_reads_return_tree_and_activity_detail_contracts()
    {
        await using var factory = new SiteWatchApiFactory();
        var folderId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var activityId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        factory.Mediator.Send(Arg.Any<ActivityCatalogTreeQuery>(), Arg.Any<CancellationToken>())
            .Returns([new ActivityCatalogNodeDto(folderId, "folder", null, "Safety", 0, null, null)]);
        factory.Mediator.Send(Arg.Any<ActivityDetailsQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ActivityDetailsDto(activityId, 7, "Inspection", "Daily", "Active", folderId, 1));
        using var client = factory.CreateHttpsClient();

        var tree = await client.GetAsync("/dashboard/activity-catalog");
        var details = await client.GetAsync($"/activities/{activityId}");

        Assert.Equal(HttpStatusCode.OK, tree.StatusCode);
        Assert.Equal("folder", (await tree.Content.ReadFromJsonAsync<JsonElement>())[0].GetProperty("kind").GetString());
        Assert.Equal(HttpStatusCode.OK, details.StatusCode);
        Assert.Equal(7, (await details.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("numberId").GetInt32());
        await factory.Mediator.Received(1).Send(Arg.Is<ActivityDetailsQuery>(query => query.ActivityId == activityId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Catalog_creates_return_the_created_resource_location_and_identifier()
    {
        await using var factory = new SiteWatchApiFactory();
        var folderId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var activityId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        factory.Mediator.Send(Arg.Any<CreateActivityFolderCommand>(), Arg.Any<CancellationToken>()).Returns(folderId);
        factory.Mediator.Send(Arg.Any<CreateActivityCommand>(), Arg.Any<CancellationToken>()).Returns(activityId);
        using var client = factory.CreateHttpsClient();

        var folder = await client.PostAsJsonAsync("/activity-folders", new { name = "Safety", parentFolderId = (Guid?)null });
        var activity = await client.PostAsJsonAsync("/activities", new { name = "Inspection", description = "Daily", parentFolderId = folderId });

        Assert.Equal(HttpStatusCode.Created, folder.StatusCode);
        Assert.Equal($"/activity-folders/{folderId}", folder.Headers.Location?.OriginalString);
        Assert.Equal(folderId, (await folder.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid());
        Assert.Equal(HttpStatusCode.Created, activity.StatusCode);
        Assert.Equal($"/activities/{activityId}", activity.Headers.Location?.OriginalString);
        Assert.Equal(activityId, (await activity.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task Catalog_mutations_use_route_identifiers_and_select_the_expected_commands()
    {
        await using var factory = new SiteWatchApiFactory();
        var folderId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var activityId = Guid.Parse("66666666-6666-6666-6666-666666666666");
        factory.Mediator.Send(Arg.Any<RenameActivityFolderCommand>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        factory.Mediator.Send(Arg.Any<MoveActivityFolderCommand>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        factory.Mediator.Send(Arg.Any<DeleteActivityFolderCommand>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        factory.Mediator.Send(Arg.Any<UpdateActivityCommand>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        factory.Mediator.Send(Arg.Any<MoveActivityCommand>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        factory.Mediator.Send(Arg.Any<SetActivityArchivedCommand>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        factory.Mediator.Send(Arg.Any<DeleteActivityCommand>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        using var client = factory.CreateHttpsClient();

        var responses = await Task.WhenAll(
            client.PutAsJsonAsync($"/activity-folders/{folderId}", new { id = Guid.NewGuid(), name = "Renamed" }),
            client.PatchAsJsonAsync($"/activity-folders/{folderId}/move", new { id = Guid.NewGuid(), targetParentFolderId = (Guid?)null, targetIndex = 0 }),
            client.DeleteAsync($"/activity-folders/{folderId}"),
            client.PutAsJsonAsync($"/activities/{activityId}", new { id = Guid.NewGuid(), name = "Updated", description = (string?)null }),
            client.PatchAsJsonAsync($"/activities/{activityId}/move", new { id = Guid.NewGuid(), targetParentFolderId = (Guid?)null, targetIndex = 0 }),
            client.PatchAsJsonAsync($"/activities/{activityId}/archive", new { }),
            client.PatchAsJsonAsync($"/activities/{activityId}/restore", new { }),
            client.DeleteAsync($"/activities/{activityId}"));

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.NoContent, response.StatusCode));
        await factory.Mediator.Received(1).Send(Arg.Is<RenameActivityFolderCommand>(command => command.Id == folderId), Arg.Any<CancellationToken>());
        await factory.Mediator.Received(1).Send(Arg.Is<MoveActivityFolderCommand>(command => command.Id == folderId && command.TargetIndex == 0), Arg.Any<CancellationToken>());
        await factory.Mediator.Received(1).Send(Arg.Is<DeleteActivityFolderCommand>(command => command.Id == folderId), Arg.Any<CancellationToken>());
        await factory.Mediator.Received(1).Send(Arg.Is<UpdateActivityCommand>(command => command.Id == activityId), Arg.Any<CancellationToken>());
        await factory.Mediator.Received(1).Send(Arg.Is<MoveActivityCommand>(command => command.Id == activityId && command.TargetIndex == 0), Arg.Any<CancellationToken>());
        await factory.Mediator.Received(1).Send(Arg.Is<SetActivityArchivedCommand>(command => command.Id == activityId && command.Archived), Arg.Any<CancellationToken>());
        await factory.Mediator.Received(1).Send(Arg.Is<SetActivityArchivedCommand>(command => command.Id == activityId && !command.Archived), Arg.Any<CancellationToken>());
        await factory.Mediator.Received(1).Send(Arg.Is<DeleteActivityCommand>(command => command.Id == activityId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Activity_details_and_requirement_creates_expose_the_requirement_contract()
    {
        await using var factory = new SiteWatchApiFactory();
        var activityId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var sectionId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var productId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var requirementId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        factory.Mediator.Send(Arg.Any<ActivityDetailsQuery>(), Arg.Any<CancellationToken>()).Returns(
            new ActivityDetailsDto(activityId, 7, "Inspection", null, "Active", null, 0,
            [new ActivityRequirementSectionDto(sectionId, "Exterior", 10m, "m2", 0,
                [new ActivityProductRequirementDto(requirementId, productId, 12, "Anchor", "Active", "Brand", "Model", 20m, "piece", 2.5m, true, "proportional", "Notes", 0)])]));
        factory.Mediator.Send(Arg.Any<CreateActivityRequirementSectionCommand>(), Arg.Any<CancellationToken>()).Returns(sectionId);
        factory.Mediator.Send(Arg.Any<CreateActivityProductRequirementCommand>(), Arg.Any<CancellationToken>()).Returns(requirementId);
        using var client = factory.CreateHttpsClient();

        var details = await client.GetAsync($"/activities/{activityId}");
        var section = await client.PostAsJsonAsync($"/activities/{activityId}/requirement-sections", new { name = "Exterior", basisQuantity = 10m, measurementUnit = "m2" });
        var product = await client.PostAsJsonAsync($"/activities/{activityId}/requirement-sections/{sectionId}/products", new { productId, quantity = 2.5m, isRequired = true, quantityBehavior = "proportional", notes = "Notes" });

        var body = await details.Content.ReadFromJsonAsync<JsonElement>();
        var returnedSection = body.GetProperty("requirementSections")[0];
        var returnedProduct = returnedSection.GetProperty("productRequirements")[0];
        Assert.Equal("m2", returnedSection.GetProperty("measurementUnit").GetString());
        Assert.Equal(productId, returnedProduct.GetProperty("productId").GetGuid());
        Assert.Equal("proportional", returnedProduct.GetProperty("quantityBehavior").GetString());
        Assert.Equal(HttpStatusCode.Created, section.StatusCode);
        Assert.Equal($"/activities/{activityId}/requirement-sections/{sectionId}", section.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.Created, product.StatusCode);
        Assert.Equal($"/activities/{activityId}/requirement-sections/{sectionId}/products/{requirementId}", product.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Requirement_mutations_use_route_identifiers_and_return_no_content()
    {
        await using var factory = new SiteWatchApiFactory();
        var activityId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var sectionId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var requirementId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        factory.Mediator.Send(Arg.Any<UpdateActivityRequirementSectionCommand>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        factory.Mediator.Send(Arg.Any<MoveActivityRequirementSectionCommand>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        factory.Mediator.Send(Arg.Any<DeleteActivityRequirementSectionCommand>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        factory.Mediator.Send(Arg.Any<UpdateActivityProductRequirementCommand>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        factory.Mediator.Send(Arg.Any<MoveActivityProductRequirementCommand>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        factory.Mediator.Send(Arg.Any<DeleteActivityProductRequirementCommand>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        using var client = factory.CreateHttpsClient();

        var responses = await Task.WhenAll(
            client.PutAsJsonAsync($"/activities/{activityId}/requirement-sections/{sectionId}", new { activityId = Guid.NewGuid(), sectionId = Guid.NewGuid(), basisQuantity = 1m, measurementUnit = "piece" }),
            client.PatchAsJsonAsync($"/activities/{activityId}/requirement-sections/{sectionId}/move", new { activityId = Guid.NewGuid(), sectionId = Guid.NewGuid(), targetIndex = 0 }),
            client.DeleteAsync($"/activities/{activityId}/requirement-sections/{sectionId}"),
            client.PutAsJsonAsync($"/activities/{activityId}/requirement-sections/{sectionId}/products/{requirementId}", new { activityId = Guid.NewGuid(), sectionId = Guid.NewGuid(), requirementId = Guid.NewGuid(), quantity = 1m, quantityBehavior = "fixed" }),
            client.PatchAsJsonAsync($"/activities/{activityId}/requirement-sections/{sectionId}/products/{requirementId}/move", new { activityId = Guid.NewGuid(), sectionId = Guid.NewGuid(), requirementId = Guid.NewGuid(), targetIndex = 0 }),
            client.DeleteAsync($"/activities/{activityId}/requirement-sections/{sectionId}/products/{requirementId}"));

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.NoContent, response.StatusCode));
        await factory.Mediator.Received(1).Send(Arg.Is<UpdateActivityRequirementSectionCommand>(command => command.ActivityId == activityId && command.SectionId == sectionId), Arg.Any<CancellationToken>());
        await factory.Mediator.Received(1).Send(Arg.Is<MoveActivityRequirementSectionCommand>(command => command.ActivityId == activityId && command.SectionId == sectionId), Arg.Any<CancellationToken>());
        await factory.Mediator.Received(1).Send(Arg.Is<DeleteActivityRequirementSectionCommand>(command => command.ActivityId == activityId && command.SectionId == sectionId), Arg.Any<CancellationToken>());
        await factory.Mediator.Received(1).Send(Arg.Is<UpdateActivityProductRequirementCommand>(command => command.ActivityId == activityId && command.SectionId == sectionId && command.RequirementId == requirementId), Arg.Any<CancellationToken>());
        await factory.Mediator.Received(1).Send(Arg.Is<MoveActivityProductRequirementCommand>(command => command.ActivityId == activityId && command.SectionId == sectionId && command.RequirementId == requirementId), Arg.Any<CancellationToken>());
        await factory.Mediator.Received(1).Send(Arg.Is<DeleteActivityProductRequirementCommand>(command => command.ActivityId == activityId && command.SectionId == sectionId && command.RequirementId == requirementId), Arg.Any<CancellationToken>());
    }
}
