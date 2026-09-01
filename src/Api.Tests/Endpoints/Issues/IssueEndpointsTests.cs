using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Tests.Infrastructure;
using Application.Issues.Commands;
using Application.Issues.Queries;
using Application.SeedWork.Models;
using NSubstitute;

namespace Api.Tests.Endpoints.Issues;

public sealed class IssueEndpointsTests
{
    [Fact]
    public async Task Create_issue_binds_the_request_and_returns_the_created_resource()
    {
        await using var factory = new SiteWatchApiFactory();
        CreateIssueCommand? captured = null;
        var issueId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        factory.Mediator.Send(Arg.Do<CreateIssueCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(issueId);
        using var client = factory.CreateHttpsClient();

        var response = await client.PostAsJsonAsync("/issues", new
        {
            siteId = "22222222-2222-2222-2222-222222222222", title = "Broken gate", description = "The gate will not close."
        });
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/issues/{issueId}", response.Headers.Location?.OriginalString);
        Assert.Equal(issueId, payload.GetProperty("id").GetGuid());
        Assert.Equal("Broken gate", captured?.Title);
    }

    [Fact]
    public async Task Issue_reads_dispatch_the_requested_identifiers_and_return_the_contract()
    {
        await using var factory = new SiteWatchApiFactory();
        var issueId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var siteId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var issue = Issue(issueId, siteId);
        factory.Mediator.Send(Arg.Any<IssueByIdQuery>(), Arg.Any<CancellationToken>()).Returns(issue);
        factory.Mediator.Send(Arg.Any<SiteIssuesQuery>(), Arg.Any<CancellationToken>()).Returns([issue]);
        using var client = factory.CreateHttpsClient();

        var detailResponse = await client.GetAsync($"/issues/{issueId}");
        var listResponse = await client.GetAsync($"/issues/site/{siteId}");
        var detail = await detailResponse.Content.ReadFromJsonAsync<JsonElement>();
        var list = await listResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        Assert.Equal(issueId, detail.GetProperty("id").GetGuid());
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.Equal("Broken gate", list[0].GetProperty("title").GetString());
        await factory.Mediator.Received(1).Send(new IssueByIdQuery(issueId), Arg.Any<CancellationToken>());
        await factory.Mediator.Received(1).Send(new SiteIssuesQuery(siteId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_issue_uses_the_route_identifier_over_the_request_body()
    {
        await using var factory = new SiteWatchApiFactory();
        UpdateIssueCommand? captured = null;
        var routeId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        factory.Mediator.Send(Arg.Do<UpdateIssueCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        using var client = factory.CreateHttpsClient();

        var response = await client.PutAsJsonAsync($"/issues/{routeId}", new
        {
            id = Guid.NewGuid(), siteId = Guid.NewGuid(), title = "Broken gate", description = "The gate will not close.",
            status = "WorkingOn", assignedWorkerIds = new[] { "worker-1" }
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(routeId, captured?.Id);
        Assert.Equal("WorkingOn", captured?.Status);
    }

    [Fact]
    public async Task Dashboard_issue_list_binds_table_state_and_returns_paged_contract()
    {
        await using var factory = new SiteWatchApiFactory();
        DashboardIssuesQuery? captured = null;
        var issue = Issue(Guid.Parse("66666666-6666-6666-6666-666666666666"), Guid.NewGuid());
        factory.Mediator.Send(Arg.Do<DashboardIssuesQuery>(query => captured = query), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<IssueDetailsDto>([issue], 1, 3));
        using var client = factory.CreateHttpsClient();

        var response = await client.GetAsync("/dashboard/issues?pageIndex=1&pageSize=50&sortActive=title&sortDirection=asc&status=Open&worker=Ada");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, payload.GetProperty("filteredCount").GetInt32());
        Assert.Equal(3, payload.GetProperty("totalCount").GetInt32());
        Assert.Equal("Broken gate", payload.GetProperty("items")[0].GetProperty("title").GetString());
        Assert.Equal(1, captured?.PageIndex);
        Assert.Equal(50, captured?.PageSize);
        Assert.Equal("title", captured?.SortActive);
        Assert.Equal("asc", captured?.SortDirection);
        Assert.Equal("Open", captured?.Status);
        Assert.Equal("Ada", captured?.Worker);
    }

    private static IssueDetailsDto Issue(Guid id, Guid siteId) => new(
        id, 42, siteId, "North site", "Broken gate", "The gate will not close.", "Open", null, null,
        DateTimeOffset.UnixEpoch, "admin", DateTimeOffset.UnixEpoch, "admin", []);
}
