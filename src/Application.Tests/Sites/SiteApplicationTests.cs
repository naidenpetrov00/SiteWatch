using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.Internal;
using Application.Sites.Commands;
using Application.Sites.Images.Commands;
using Application.Sites.Images.Queries;
using Application.Sites.Queries;
using Application.Sites.Videos.Commands;
using Application.Sites.Videos.Queries;
using Domain.SeedWork.Enums;
using Domain.ValueObjects;
using NSubstitute;
using ImageUploadedFile = Application.Sites.Images.Commands.UploadedFile;
using VideoUploadedFile = Application.Sites.Videos.Commands.UploadedFile;

namespace Application.Tests.Sites;

public sealed class SiteApplicationTests
{
    [Fact]
    public async Task Create_site_validation_accepts_the_new_contract()
    {
        var command = new CreateSiteCommand
        {
            Name = "Apartment 42",
            Address = "42 Main Street",
            ManagerId = "manager-1",
            StartDate = "2026-03-03",
            MediaPolicyPreset = " apartmentrenovation ",
            MediaCategories = ["Design", "Demolition", "Electricity", "Pipes", "Finishes", "Other"],
        };

        var result = await new CreateSiteValidator().ValidateAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("Regular")]
    [InlineData("Unsupported")]
    [InlineData("")]
    public async Task Create_site_validation_rejects_legacy_and_unsupported_presets(string preset)
    {
        var command = ValidCreateCommand() with { MediaPolicyPreset = preset };

        var result = await new CreateSiteValidator().ValidateAsync(command);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateSiteCommand.MediaPolicyPreset));
    }

    [Fact]
    public async Task Create_site_validation_returns_category_failures_instead_of_domain_exceptions()
    {
        var validator = new CreateSiteValidator();
        var reserved = await validator.ValidateAsync(
            ValidCreateCommand() with { MediaCategories = ["All"] });
        var oversized = await validator.ValidateAsync(
            ValidCreateCommand() with
            {
                MediaCategories = [new string('x', SiteMediaPolicy.MaxCategoryLength + 1)],
            });
        var excess = await validator.ValidateAsync(
            ValidCreateCommand() with
            {
                MediaCategories = Enumerable.Range(1, SiteMediaPolicy.MaxCategoryCount)
                    .Select(index => $"Category {index}")
                    .ToArray(),
            });

        Assert.Contains(reserved.Errors, error => error.PropertyName == nameof(CreateSiteCommand.MediaCategories));
        Assert.Contains(oversized.Errors, error => error.PropertyName == nameof(CreateSiteCommand.MediaCategories));
        Assert.Contains(excess.Errors, error => error.PropertyName == nameof(CreateSiteCommand.MediaCategories));
    }

    [Fact]
    public async Task Create_site_validation_requires_a_category_collection()
    {
        var command = ValidCreateCommand() with { MediaCategories = null! };

        var result = await new CreateSiteValidator().ValidateAsync(command);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateSiteCommand.MediaCategories));
    }

    [Theory]
    [InlineData(null, "Category is not valid.")]
    [InlineData("   ", "Category is not valid.")]
    [InlineData(
        "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
        "Category cannot exceed 50 characters.")]
    public async Task Image_and_video_validation_reject_invalid_string_categories(
        string? category,
        string message)
    {
        var dbContext = Substitute.For<IApplicationDbContext>();
        await using var imageStream = new MemoryStream([1]);
        await using var videoStream = new MemoryStream([1]);
        var image = new AddImageCommand(
            Guid.Empty,
            new ImageUploadedFile { Stream = imageStream, ContentType = "image/jpeg" },
            category);
        var video = new AddVideoCommand(
            Guid.Empty,
            new VideoUploadedFile { Stream = videoStream, ContentType = "video/mp4" },
            category);

        var imageResult = await new AddImageValidator(dbContext).ValidateAsync(image);
        var videoResult = await new AddVideoValidator(dbContext).ValidateAsync(video);

        Assert.Contains(imageResult.Errors, error =>
            error.PropertyName == nameof(AddImageCommand.Category)
            && error.ErrorMessage == message);
        Assert.Contains(videoResult.Errors, error =>
            error.PropertyName == nameof(AddVideoCommand.Category)
            && error.ErrorMessage == message);
    }

    [Fact]
    public async Task Preset_query_returns_the_public_preset_contract_in_definition_order()
    {
        var result = await new SiteMediaPolicyPresetsQueryHandler()
            .Handle(new SiteMediaPolicyPresetsQuery(), CancellationToken.None);

        Assert.Equal(
            ["ApartmentRenovation", "HouseBuild", "CommercialBuild", "SiteMaintenance", "Custom"],
            result.Select(item => item.Preset));
        Assert.Equal("Apartment Renovation", result[0].DisplayName);
        Assert.Equal(
            ["Design", "Demolition", "Electricity", "Pipes", "Finishes", "Other"],
            result[0].Categories);
        Assert.Equal(["Other"], result[^1].Categories);
    }

    [Fact]
    public async Task Site_handlers_delegate_create_and_update_contracts_with_cancellation()
    {
        var sites = Substitute.For<ISiteService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var create = ValidCreateCommand();
        var update = new UpdateSiteCommand
        {
            Id = Guid.NewGuid(),
            Name = "Apartment 42",
            Address = "42 Main Street",
            MediaCategoriesToAdd = ["Access Control"],
        };
        var createdId = Guid.NewGuid();
        sites.CreateAsync(create, cancellationToken).Returns(createdId);

        var result = await new CreateSiteHandler(sites).Handle(create, cancellationToken);
        await new UpdateSiteHandler(sites).Handle(update, cancellationToken);

        Assert.Equal(createdId, result);
        await sites.Received(1).CreateAsync(create, cancellationToken);
        await sites.Received(1).UpdateAsync(update, cancellationToken);
    }

    [Fact]
    public async Task Add_image_forwards_custom_category_and_uploaded_identifiers()
    {
        var blobs = Substitute.For<IBlobService>();
        var images = Substitute.For<IImagesService>();
        var cancellationToken = new CancellationTokenSource().Token;
        await using var stream = new MemoryStream([1, 2, 3]);
        var siteId = Guid.NewGuid();
        var uploaded = new UploadedImageResult(Guid.NewGuid(), Guid.NewGuid());
        var command = new AddImageCommand(
            siteId,
            new ImageUploadedFile { Stream = stream, ContentType = "image/jpeg" },
            "HVAC Controls");
        blobs.UploadImageAsync(stream, "image/jpeg", BlobContainerName.Images, cancellationToken)
            .Returns(uploaded);

        var result = await new AddImageHandler(blobs, images).Handle(command, cancellationToken);

        Assert.Equal(uploaded, result);
        await images.Received(1).AddImageIdsToSiteAsync(
            siteId,
            uploaded.OriginalFileId,
            uploaded.ThumbnailFileId,
            "HVAC Controls",
            cancellationToken);
    }

    [Fact]
    public async Task Add_video_forwards_custom_category_duration_and_uploaded_identifiers()
    {
        var blobs = Substitute.For<IVideosBlobService>();
        var videos = Substitute.For<IVideosService>();
        var cancellationToken = new CancellationTokenSource().Token;
        await using var stream = new MemoryStream([1, 2, 3]);
        var siteId = Guid.NewGuid();
        var uploaded = new UploadedVideoResult(Guid.NewGuid(), Guid.NewGuid(), 37);
        var command = new AddVideoCommand(
            siteId,
            new VideoUploadedFile { Stream = stream, ContentType = "video/mp4" },
            "Access Control");
        blobs.UploadVideoAsync(stream, "video/mp4", BlobContainerName.Videos, cancellationToken)
            .Returns(uploaded);

        var result = await new AddVideoHandler(blobs, videos).Handle(command, cancellationToken);

        Assert.Equal(uploaded, result);
        await videos.Received(1).AddVideoIdsToSiteAsync(
            siteId,
            uploaded.VideoFileId,
            uploaded.SnapshotFileId,
            37,
            "Access Control",
            cancellationToken);
    }

    [Fact]
    public async Task Media_id_queries_forward_the_request_and_cancellation_token()
    {
        var images = Substitute.For<IImagesService>();
        var videos = Substitute.For<IVideosService>();
        var siteId = Guid.NewGuid();
        var cancellationToken = new CancellationTokenSource().Token;
        var imageItems = new List<SiteImageIdsDto>();
        var videoItems = new List<SiteVideoIdsDto>();
        images.GetImagesIdsBySiteId(siteId, cancellationToken).Returns(imageItems);
        videos.GetVideosIdsBySiteId(siteId, cancellationToken).Returns(videoItems);

        var returnedImages = await new GetImagesIdsBySiteIdHandler(images).Handle(
            new GetImagesIdsBySiteIdQuery { SiteId = siteId },
            cancellationToken);
        var returnedVideos = await new GetVideosIdsBySiteIdHandler(videos).Handle(
            new GetVideosIdsBySiteIdQuery { SiteId = siteId },
            cancellationToken);

        Assert.Same(imageItems, returnedImages);
        Assert.Same(videoItems, returnedVideos);
    }

    [Fact]
    public void Site_dtos_expose_media_policy_as_preset_and_shared_categories()
    {
        var policy = SiteMediaPolicy.FromPreset(MediaPolicyPreset.HouseBuild);
        var site = new Domain.Entities.Site(
            "House 12",
            "12 Main Street",
            "manager-1",
            new DateOnly(2026, 3, 3),
            mediaPolicy: policy);

        var dashboardDto = DashboardSiteDto.From(site);
        var policyDto = SiteMediaPolicyDto.From(policy);

        Assert.Equal("HouseBuild", dashboardDto.MediaPolicy.Preset);
        Assert.Equal(policy.Categories, dashboardDto.MediaPolicy.Categories);
        Assert.Equal("HouseBuild", policyDto.Preset);
        Assert.Equal(policy.Categories, policyDto.Categories);
    }

    private static CreateSiteCommand ValidCreateCommand() => new()
    {
        Name = "Apartment 42",
        Address = "42 Main Street",
        MediaPolicyPreset = "ApartmentRenovation",
        MediaCategories = ["Design", "Demolition", "Electricity", "Pipes", "Finishes", "Other"],
    };
}
