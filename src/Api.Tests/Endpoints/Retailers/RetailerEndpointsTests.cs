using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Tests.Infrastructure;
using Application.Persons.Queries;
using Application.Retailers.Commands;
using Application.Retailers.Queries;
using Application.SeedWork.Models;
using NSubstitute;

namespace Api.Tests.Endpoints.Retailers;

public sealed class RetailerEndpointsTests
{
    [Fact]
    public async Task Retailer_writes_bind_the_contract_and_prefer_route_identifiers()
    {
        await using var factory = new SiteWatchApiFactory();
        var retailerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        CreateRetailerCommand? create = null;
        UpdateRetailerCommand? update = null;
        factory.Mediator.Send(Arg.Do<CreateRetailerCommand>(command => create = command), Arg.Any<CancellationToken>()).Returns(retailerId);
        factory.Mediator.Send(Arg.Do<UpdateRetailerCommand>(command => update = command), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        factory.Mediator.Send(Arg.Any<SetRetailerActiveCommand>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        using var client = factory.CreateHttpsClient();

        var created = await client.PostAsJsonAsync("/retailers", Request());
        var updated = await client.PutAsJsonAsync($"/retailers/{retailerId}", new { id = Guid.NewGuid(), displayName = "Updated Store", companyPersonId = Guid.NewGuid(), baseWebsiteUrl = "https://updated.example", notes = "Updated" });
        var deactivated = await client.PatchAsJsonAsync($"/retailers/{retailerId}/deactivate", new { });
        var activated = await client.PatchAsJsonAsync($"/retailers/{retailerId}/activate", new { });

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal($"/retailers/{retailerId}", created.Headers.Location?.OriginalString);
        Assert.Equal(retailerId, (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid());
        Assert.Equal("Example Store", create?.DisplayName);
        Assert.Equal(HttpStatusCode.NoContent, updated.StatusCode);
        Assert.Equal(retailerId, update?.Id);
        Assert.All([deactivated, activated], response => Assert.Equal(HttpStatusCode.NoContent, response.StatusCode));
        await factory.Mediator.Received(1).Send(Arg.Is<SetRetailerActiveCommand>(command => command.Id == retailerId && !command.IsActive), Arg.Any<CancellationToken>());
        await factory.Mediator.Received(1).Send(Arg.Is<SetRetailerActiveCommand>(command => command.Id == retailerId && command.IsActive), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Retailer_reads_and_company_lookup_bind_dashboard_query_contracts()
    {
        await using var factory = new SiteWatchApiFactory();
        var retailerId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        factory.Mediator.Send(Arg.Any<RetailerByIdQuery>(), Arg.Any<CancellationToken>()).Returns(Details(retailerId));
        factory.Mediator.Send(Arg.Any<DashboardRetailersQuery>(), Arg.Any<CancellationToken>()).Returns(new PagedResult<RetailerTableDto>([Table(retailerId)], 1, 2));
        factory.Mediator.Send(Arg.Any<RetailerSearchQuery>(), Arg.Any<CancellationToken>()).Returns([new RetailerLookupDto { Id = retailerId, DisplayName = "Example Store", BaseWebsiteUrl = "https://example.com", WebsiteHost = "example.com" }]);
        factory.Mediator.Send(Arg.Any<CompanyPersonSearchQuery>(), Arg.Any<CancellationToken>()).Returns([new CompanyPersonLookupDto { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), DisplayName = "Example Ltd", Eik = "123456789", VatNumber = "BG123" }]);
        using var client = factory.CreateHttpsClient();

        var detail = await client.GetAsync($"/retailers/{retailerId}");
        var table = await client.GetAsync("/dashboard/retailers?pageIndex=1&pageSize=50&displayName=store&isActive=true&sortActive=websiteHost&sortDirection=asc");
        var lookup = await client.GetAsync("/dashboard/retailers/search?searchTerm=example");
        var companies = await client.GetAsync("/dashboard/persons/companies/search?searchTerm=example");

        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        Assert.Equal("Example Store", (await detail.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("displayName").GetString());
        Assert.Equal(2, (await table.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("totalCount").GetInt32());
        Assert.Equal("example.com", (await lookup.Content.ReadFromJsonAsync<JsonElement>())[0].GetProperty("websiteHost").GetString());
        Assert.Equal("Example Ltd", (await companies.Content.ReadFromJsonAsync<JsonElement>())[0].GetProperty("displayName").GetString());
        await factory.Mediator.Received(1).Send(Arg.Is<DashboardRetailersQuery>(query => query.PageIndex == 1 && query.PageSize == 50 && query.DisplayName == "store" && query.IsActive == true && query.SortActive == "websiteHost"), Arg.Any<CancellationToken>());
        await factory.Mediator.Received(1).Send(Arg.Is<RetailerSearchQuery>(query => query.SearchTerm == "example"), Arg.Any<CancellationToken>());
        await factory.Mediator.Received(1).Send(Arg.Is<CompanyPersonSearchQuery>(query => query.SearchTerm == "example"), Arg.Any<CancellationToken>());
    }

    private static object Request() => new { displayName = "Example Store", companyPersonId = Guid.Parse("33333333-3333-3333-3333-333333333333"), baseWebsiteUrl = "https://example.com", notes = "Notes" };

    private static RetailerDetailsDto Details(Guid id) => new() { Id = id, DisplayName = "Example Store", BaseWebsiteUrl = "https://example.com", WebsiteHost = "example.com", IsActive = true, CompanyPerson = new CompanyPersonLookupDto { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), DisplayName = "Example Ltd", Eik = "123456789", VatNumber = "BG123" } };

    private static RetailerTableDto Table(Guid id) => new() { Id = id, DisplayName = "Example Store", CompanyPersonId = Guid.Parse("33333333-3333-3333-3333-333333333333"), CompanyDisplayName = "Example Ltd", BaseWebsiteUrl = "https://example.com", WebsiteHost = "example.com", IsActive = true };
}
