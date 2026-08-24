using Application.Cameras.Commands;
using Application.SeedWork.Interfaces;
using NSubstitute;

namespace Application.Tests.Cameras;

public sealed class CameraCommandTests
{
    [Theory]
    [InlineData("Http")]
    [InlineData("https")]
    public async Task Dashboard_camera_upsert_accepts_supported_connection_boundaries(string protocol)
    {
        var command = ValidCamera() with { Protocol = protocol, RtspPort = 1, PtzPort = 65535 };

        var result = await new CameraUpsertValidator<CreateDashboardCameraCommand>().ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("Unknown", "Https", 554, 443, nameof(CreateDashboardCameraCommand.Brand))]
    [InlineData("Dahua", "Ftp", 554, 443, nameof(CreateDashboardCameraCommand.Protocol))]
    [InlineData("Dahua", "Https", 0, 443, nameof(CreateDashboardCameraCommand.RtspPort))]
    [InlineData("Dahua", "Https", 554, 65536, nameof(CreateDashboardCameraCommand.PtzPort))]
    public async Task Dashboard_camera_upsert_rejects_invalid_connection_details(
        string brand,
        string protocol,
        int rtspPort,
        int ptzPort,
        string invalidProperty)
    {
        var command = ValidCamera() with
        {
            Brand = brand,
            Protocol = protocol,
            RtspPort = rtspPort,
            PtzPort = ptzPort,
            SiteId = Guid.Empty,
        };

        var result = await new CameraUpsertValidator<CreateDashboardCameraCommand>().ValidateAsync(command);

        Assert.Contains(result.Errors, error => error.PropertyName == invalidProperty);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateDashboardCameraCommand.SiteId));
    }

    [Theory]
    [InlineData("Forward")]
    [InlineData(" ")]
    public async Task Ptz_direction_commands_reject_unsupported_directions(string direction)
    {
        var start = await new StartPtzMovementCommandValidator()
            .ValidateAsync(new StartPtzMovementCommand(Guid.NewGuid(), direction));
        var stop = await new StopPtzMovementCommandValidator()
            .ValidateAsync(new StopPtzMovementCommand(Guid.NewGuid(), direction));

        Assert.Contains(start.Errors, error => error.PropertyName == nameof(StartPtzMovementCommand.Direction));
        Assert.Contains(stop.Errors, error => error.PropertyName == nameof(StopPtzMovementCommand.Direction));
    }

    [Theory]
    [InlineData(-1d, 1d, 0d)]
    [InlineData(-1.01d, 0d, 0d)]
    [InlineData(0d, 1.01d, 0d)]
    public async Task Relative_ptz_command_enforces_normalized_offsets(double horizontal, double vertical, double zoom)
    {
        var result = await new MovePtzRelativelyCommandValidator()
            .ValidateAsync(new MovePtzRelativelyCommand(Guid.NewGuid(), horizontal, vertical, zoom));

        Assert.Equal(horizontal is >= -1 and <= 1 && vertical is >= -1 and <= 1 && zoom is >= -1 and <= 1, result.IsValid);
    }

    [Fact]
    public async Task Ptz_handlers_normalize_directions_before_delegating_to_the_camera_service()
    {
        var cameras = Substitute.For<ICameraService>();
        var cameraId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        await new StartPtzMovementCommandHandler(cameras)
            .Handle(new StartPtzMovementCommand(cameraId, " left "), CancellationToken.None);
        await new StopPtzMovementCommandHandler(cameras)
            .Handle(new StopPtzMovementCommand(cameraId, "DOWN"), CancellationToken.None);

        await cameras.Received(1).StartPtzMovementAsync(cameraId, "Left", CancellationToken.None);
        await cameras.Received(1).StopPtzMovementAsync(cameraId, "Down", CancellationToken.None);
    }

    [Fact]
    public async Task Relative_ptz_handler_preserves_each_normalized_offset()
    {
        var cameras = Substitute.For<ICameraService>();
        var cameraId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        await new MovePtzRelativelyCommandHandler(cameras)
            .Handle(new MovePtzRelativelyCommand(cameraId, -1, 0.5, 1), CancellationToken.None);

        await cameras.Received(1).MovePtzRelativelyAsync(cameraId, -1, 0.5, 1, CancellationToken.None);
    }

    private static CreateDashboardCameraCommand ValidCamera() => new()
    {
        Name = "North gate",
        Brand = "Dahua",
        Model = "IPC-HDW",
        Username = "operator",
        Password = "safe-password",
        IpAddress = "192.0.2.10",
        RtspPort = 554,
        PtzPort = 443,
        Protocol = "Https",
        SiteId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
    };
}
