using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Tests.Infrastructure;
using Application.Cameras.Commands;
using Application.Cameras.Queries;
using Application.SeedWork.Models;
using Application.SeedWork.Models.Internal;
using NSubstitute;

namespace Api.Tests.Endpoints.Cameras;

public sealed class CameraEndpointsTests
{
    [Fact]
    public async Task Dashboard_camera_list_returns_the_paged_administrator_contract()
    {
        await using var factory = new SiteWatchApiFactory();
        factory.Mediator.Send(Arg.Any<DashboardCamerasQuery>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<DashboardCameraDto>(
                [
                    new(Guid.Parse("11111111-1111-1111-1111-111111111111"), 7, "North gate", "Dahua", "IPC-HDW",
                        "192.0.2.10", 554, 443, "Https", Guid.NewGuid(), "Head office")
                ],
                1,
                3));
        using var client = factory.CreateHttpsClient();

        var response = await client.GetAsync("/dashboard/cameras?pageIndex=1&pageSize=50&name=gate");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("North gate", payload.GetProperty("items")[0].GetProperty("name").GetString());
        Assert.Equal(1, payload.GetProperty("filteredCount").GetInt32());
        Assert.Equal(3, payload.GetProperty("totalCount").GetInt32());
        await factory.Mediator.Received(1).Send(
            Arg.Is<DashboardCamerasQuery>(query =>
                query.PageIndex == 1 && query.PageSize == 50 && query.Name == "gate"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Dashboard_camera_detail_returns_the_editable_connection_contract()
    {
        await using var factory = new SiteWatchApiFactory();
        var cameraId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        factory.Mediator.Send(Arg.Any<DashboardCameraByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(new DashboardCameraDetailsDto(cameraId, 8, "Loading bay", "Dahua", "IPC-HDW", "operator", "secret",
                "192.0.2.11", 554, 443, "Http", Guid.NewGuid(), "Warehouse"));
        using var client = factory.CreateHttpsClient();

        var response = await client.GetAsync($"/dashboard/cameras/{cameraId}");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("operator", payload.GetProperty("username").GetString());
        Assert.Equal("Http", payload.GetProperty("protocol").GetString());
    }

    [Fact]
    public async Task Create_camera_binds_the_dashboard_request_and_returns_its_resource_location()
    {
        await using var factory = new SiteWatchApiFactory();
        CreateDashboardCameraCommand? captured = null;
        var cameraId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        factory.Mediator.Send(Arg.Do<CreateDashboardCameraCommand>(command => captured = command),
            Arg.Any<CancellationToken>()).Returns(cameraId);
        using var client = factory.CreateHttpsClient();

        var response = await client.PostAsJsonAsync("/cameras", CameraRequest());
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/cameras/{cameraId}", response.Headers.Location?.OriginalString);
        Assert.Equal(cameraId, payload.GetProperty("id").GetGuid());
        Assert.Equal("Dahua", captured?.Brand);
        Assert.Equal(443, captured?.PtzPort);
    }

    [Fact]
    public async Task Update_camera_uses_the_route_identifier_over_a_body_identifier()
    {
        await using var factory = new SiteWatchApiFactory();
        UpdateDashboardCameraCommand? captured = null;
        var routeId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        factory.Mediator
            .Send(Arg.Do<UpdateDashboardCameraCommand>(command => captured = command), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        using var client = factory.CreateHttpsClient();

        var request = CameraRequest();
        var response = await client.PutAsJsonAsync($"/cameras/{routeId}", new
        {
            id = Guid.NewGuid(),
            request.Name,
            request.Brand,
            request.Model,
            request.Username,
            request.Password,
            request.IpAddress,
            request.RtspPort,
            request.PtzPort,
            request.Protocol,
            request.SiteId,
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(routeId, captured?.Id);
    }

    [Fact]
    public async Task Delete_camera_dispatches_the_route_identifier()
    {
        await using var factory = new SiteWatchApiFactory();
        var cameraId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        factory.Mediator.Send(Arg.Any<DeleteCameraCommand>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        using var client = factory.CreateHttpsClient();

        var response = await client.DeleteAsync($"/cameras/{cameraId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        await factory.Mediator.Received(1).Send(new DeleteCameraCommand(cameraId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Ptz_endpoints_bind_the_camera_identifier_and_request_body()
    {
        await using var factory = new SiteWatchApiFactory();
        var cameraId = Guid.Parse("77777777-7777-7777-7777-777777777777");
        factory.Mediator.Send(Arg.Any<StartPtzMovementCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        factory.Mediator.Send(Arg.Any<StopPtzMovementCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        factory.Mediator.Send(Arg.Any<MovePtzRelativelyCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        using var client = factory.CreateHttpsClient();

        var start = await client.PostAsJsonAsync($"/cameras/{cameraId}/ptz/start", new { direction = "Left" });
        var stop = await client.PostAsJsonAsync($"/cameras/{cameraId}/ptz/stop", new { direction = "Down" });
        var relative = await client.PostAsJsonAsync($"/cameras/{cameraId}/ptz/relative",
            new { horizontal = -1d, vertical = 0.5d, zoom = 1d });

        Assert.Equal(HttpStatusCode.NoContent, start.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, stop.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, relative.StatusCode);
        await factory.Mediator.Received(1)
            .Send(new StartPtzMovementCommand(cameraId, "Left"), Arg.Any<CancellationToken>());
        await factory.Mediator.Received(1)
            .Send(new StopPtzMovementCommand(cameraId, "Down"), Arg.Any<CancellationToken>());
        await factory.Mediator.Received(1)
            .Send(new MovePtzRelativelyCommand(cameraId, -1, 0.5, 1), Arg.Any<CancellationToken>());
    }

    private static CameraRequestBody CameraRequest() => new(
        "North gate",
        "Dahua",
        "IPC-HDW",
        "operator",
        "secret",
        "192.0.2.10",
        554,
        443,
        "Https",
        Guid.Parse("88888888-8888-8888-8888-888888888888"));

    private sealed record CameraRequestBody(
        string Name,
        string Brand,
        string Model,
        string Username,
        string Password,
        string IpAddress,
        int RtspPort,
        int PtzPort,
        string Protocol,
        Guid SiteId);
}