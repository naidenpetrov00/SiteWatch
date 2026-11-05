using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Tests.Infrastructure;
using Application.TerritorialStructure;
using NSubstitute;

namespace Api.Tests.Endpoints.TerritorialStructure;

public sealed class TerritorialStructureEndpointsTests
{
    [Fact]
    public async Task Search_binds_the_hierarchy_filters_and_returns_the_entry_contract()
    {
        await using var factory = new SiteWatchApiFactory();
        var municipalityId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var settlementId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        factory.Mediator.Send(Arg.Any<TerritorialEntriesQuery>(), Arg.Any<CancellationToken>())
            .Returns([new TerritorialEntryDto(settlementId, "Sofia", false, municipalityId)]);
        using var client = factory.CreateHttpsClient();

        var response = await client.GetAsync(
            $"/territorial-structure?kind=1&parentId={municipalityId}&searchTerm=sof&includeInactive=true");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(settlementId, body[0].GetProperty("id").GetGuid());
        Assert.Equal("Sofia", body[0].GetProperty("name").GetString());
        Assert.False(body[0].GetProperty("isActive").GetBoolean());
        Assert.Equal(municipalityId, body[0].GetProperty("parentId").GetGuid());
        await factory.Mediator.Received(1).Send(
            Arg.Is<TerritorialEntriesQuery>(query =>
                query.Kind == TerritorialEntryKind.Settlement
                && query.ParentId == municipalityId
                && query.SearchTerm == "sof"
                && query.IncludeInactive),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_returns_the_created_identifier_and_resource_location()
    {
        await using var factory = new SiteWatchApiFactory();
        var municipalityId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var settlementId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        factory.Mediator.Send(Arg.Any<CreateTerritorialEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(settlementId);
        using var client = factory.CreateHttpsClient();

        var response = await client.PostAsJsonAsync("/territorial-structure", new
        {
            kind = TerritorialEntryKind.Settlement,
            parentId = municipalityId,
            name = "Sofia"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/territorial-structure/{settlementId}", response.Headers.Location?.OriginalString);
        Assert.Equal(settlementId,
            (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid());
        await factory.Mediator.Received(1).Send(
            Arg.Is<CreateTerritorialEntryCommand>(command =>
                command.Kind == TerritorialEntryKind.Settlement
                && command.ParentId == municipalityId
                && command.Name == "Sofia"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_and_delete_use_route_and_query_identifiers()
    {
        await using var factory = new SiteWatchApiFactory();
        var entryId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        factory.Mediator.Send(Arg.Any<UpdateTerritorialEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        factory.Mediator.Send(Arg.Any<DeleteTerritorialEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        using var client = factory.CreateHttpsClient();

        var update = await client.PutAsJsonAsync($"/territorial-structure/{entryId}", new
        {
            id = Guid.NewGuid(),
            kind = TerritorialEntryKind.District,
            name = "Oborishte",
            isActive = false
        });
        var delete = await client.DeleteAsync(
            $"/territorial-structure/{entryId}?kind={(int)TerritorialEntryKind.District}");

        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        await factory.Mediator.Received(1).Send(
            Arg.Is<UpdateTerritorialEntryCommand>(command =>
                command.Id == entryId
                && command.Kind == TerritorialEntryKind.District
                && command.Name == "Oborishte"
                && !command.IsActive),
            Arg.Any<CancellationToken>());
        await factory.Mediator.Received(1).Send(
            Arg.Is<DeleteTerritorialEntryCommand>(command =>
                command.Id == entryId && command.Kind == TerritorialEntryKind.District),
            Arg.Any<CancellationToken>());
    }
}
