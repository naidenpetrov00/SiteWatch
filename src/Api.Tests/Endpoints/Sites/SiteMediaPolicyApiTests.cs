using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Tests.Infrastructure;
using Application.Sites.Commands;
using Application.Sites.Images.Commands;
using Application.Sites.Queries;
using Application.Sites.Videos.Commands;
using NSubstitute;

namespace Api.Tests.Endpoints.Sites;

public sealed class SiteMediaPolicyApiTests
{
    [Fact]
    public async Task Preset_endpoint_returns_the_administrator_contract()
    {
        await using var factory = new SiteWatchApiFactory();
        IReadOnlyList<SiteMediaPolicyPresetDto> presets =
        [
            new(
                "ApartmentRenovation",
                "Apartment Renovation",
                ["Design", "Demolition", "Electricity", "Pipes", "Finishes", "Other"]),
            new("Custom", "Custom", ["Other"]),
        ];
        factory.Mediator
            .Send(Arg.Any<SiteMediaPolicyPresetsQuery>(), Arg.Any<CancellationToken>())
            .Returns(presets);
        using var client = factory.CreateHttpsClient();

        var response = await client.GetAsync("/dashboard/sites/media-policy-presets");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("ApartmentRenovation", payload[0].GetProperty("preset").GetString());
        Assert.Equal("Apartment Renovation", payload[0].GetProperty("displayName").GetString());
        Assert.Equal(
            ["Design", "Demolition", "Electricity", "Pipes", "Finishes", "Other"],
            payload[0].GetProperty("categories").EnumerateArray().Select(item => item.GetString()));
        Assert.Equal("Custom", payload[1].GetProperty("preset").GetString());
    }

    [Fact]
    public async Task Create_site_binds_preset_and_categories_and_returns_the_created_contract()
    {
        await using var factory = new SiteWatchApiFactory();
        CreateSiteCommand? captured = null;
        var siteId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        factory.Mediator
            .Send(
                Arg.Do<CreateSiteCommand>(command => captured = command),
                Arg.Any<CancellationToken>())
            .Returns(siteId);
        using var client = factory.CreateHttpsClient();

        var response = await client.PostAsJsonAsync("/sites", new
        {
            name = "Apartment 42",
            address = "42 Main Street",
            mediaPolicyPreset = "Custom",
            mediaCategories = new[] { "HVAC Controls", "Other" },
        });
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/dashboard/sites/{siteId}", response.Headers.Location?.OriginalString);
        Assert.Equal(siteId, payload.GetProperty("id").GetGuid());
        Assert.NotNull(captured);
        Assert.Equal("Custom", captured.MediaPolicyPreset);
        Assert.Equal(["HVAC Controls", "Other"], captured.MediaCategories);
    }

    [Fact]
    public async Task Update_site_uses_the_route_identifier_and_binds_category_additions()
    {
        await using var factory = new SiteWatchApiFactory();
        UpdateSiteCommand? captured = null;
        var routeId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        factory.Mediator
            .Send(
                Arg.Do<UpdateSiteCommand>(command => captured = command),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        using var client = factory.CreateHttpsClient();

        var response = await client.PutAsJsonAsync($"/dashboard/sites/{routeId}", new
        {
            id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            name = "Apartment 42",
            address = "42 Main Street",
            mediaCategoriesToAdd = new[] { "Access Control" },
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotNull(captured);
        Assert.Equal(routeId, captured.Id);
        Assert.Equal(["Access Control"], captured.MediaCategoriesToAdd);
    }

    [Fact]
    public async Task Dashboard_site_serializes_media_policy_as_a_nested_object()
    {
        await using var factory = new SiteWatchApiFactory();
        var siteId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var site = new DashboardSiteDto(
            siteId,
            42,
            "House 42",
            "42 Main Street",
            new SiteMediaPolicyDto
            {
                Preset = "HouseBuild",
                Categories = ["Design", "Foundation", "Structure", "Other"],
            });
        factory.Mediator
            .Send(Arg.Any<DashboardSiteByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(site);
        using var client = factory.CreateHttpsClient();

        var response = await client.GetAsync($"/dashboard/sites/{siteId}");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var mediaPolicy = payload.GetProperty("mediaPolicy");
        Assert.Equal("HouseBuild", mediaPolicy.GetProperty("preset").GetString());
        Assert.Equal(
            ["Design", "Foundation", "Structure", "Other"],
            mediaPolicy.GetProperty("categories").EnumerateArray().Select(item => item.GetString()));
        Assert.False(mediaPolicy.TryGetProperty("allowedImageCategories", out _));
        Assert.False(mediaPolicy.TryGetProperty("allowedVideoCategories", out _));
    }

    [Fact]
    public async Task Image_upload_binds_a_free_form_category_string()
    {
        await using var factory = new SiteWatchApiFactory();
        AddImageCommand? captured = null;
        var result = new UploadedImageResult(Guid.NewGuid(), Guid.NewGuid());
        factory.Mediator
            .Send(
                Arg.Do<AddImageCommand>(command => captured = command),
                Arg.Any<CancellationToken>())
            .Returns(result);
        using var client = factory.CreateHttpsClient();
        using var content = MediaForm("image/jpeg", "photo.jpg", "HVAC Controls");

        var response = await client.PostAsync($"/images/{Guid.NewGuid()}", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("HVAC Controls", captured?.Category);
    }

    [Fact]
    public async Task Video_upload_binds_a_free_form_category_string()
    {
        await using var factory = new SiteWatchApiFactory();
        AddVideoCommand? captured = null;
        var result = new UploadedVideoResult(Guid.NewGuid(), Guid.NewGuid(), 12);
        factory.Mediator
            .Send(
                Arg.Do<AddVideoCommand>(command => captured = command),
                Arg.Any<CancellationToken>())
            .Returns(result);
        using var client = factory.CreateHttpsClient();
        using var content = MediaForm("video/mp4", "clip.mp4", "Access Control");

        var response = await client.PostAsync($"/videos/{Guid.NewGuid()}", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Access Control", captured?.Category);
    }

    private static MultipartFormDataContent MediaForm(
        string contentType,
        string fileName,
        string category)
    {
        var content = new MultipartFormDataContent();
        var file = new ByteArrayContent([1, 2, 3]);
        file.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        content.Add(file, "file", fileName);
        content.Add(new StringContent(category), "category");
        return content;
    }
}
