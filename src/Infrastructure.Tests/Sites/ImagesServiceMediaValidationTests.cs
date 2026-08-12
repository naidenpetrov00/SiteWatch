using Application.SeedWork.Interfaces;
using FluentValidation;
using Infrastructure.Sites.Services;
using NSubstitute;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;

namespace Infrastructure.Tests.Sites;

public sealed class ImagesServiceMediaValidationTests
{
    private readonly ImagesService _service = new(Substitute.For<IApplicationDbContext>());

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    [InlineData("image/gif")]
    public async Task CreateThumbnailAsync_accepts_fully_decodable_matching_images(string contentType)
    {
        await using var image = await CreateImageSharpImageAsync(contentType);

        await using var thumbnail = await _service.CreateThumbnailAsync(
            image,
            contentType,
            CancellationToken.None);

        Assert.True(thumbnail.Length > 0);
        Assert.Equal(0, thumbnail.Position);
        Assert.Equal(0xFF, thumbnail.ReadByte());
        Assert.Equal(0xD8, thumbnail.ReadByte());
    }

    [Theory]
    [InlineData("image/heic")]
    [InlineData("image/heif")]
    public async Task CreateThumbnailAsync_accepts_heic_and_heif_as_compatible_variants(
        string contentType)
    {
        await using var image = CreateHeicSample();

        await using var thumbnail = await _service.CreateThumbnailAsync(
            image,
            contentType,
            CancellationToken.None);

        Assert.True(thumbnail.Length > 0);
        Assert.Equal(0, thumbnail.Position);
    }

    [Fact]
    public async Task CreateThumbnailAsync_rejects_a_declared_type_that_does_not_match_the_image()
    {
        await using var image = await CreateImageSharpImageAsync("image/png");

        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateThumbnailAsync(
            image,
            "image/jpeg",
            CancellationToken.None));
    }

    [Fact]
    public async Task CreateThumbnailAsync_rejects_corrupt_image_content()
    {
        await using var image = new MemoryStream("not an image"u8.ToArray());

        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateThumbnailAsync(
            image,
            "image/jpeg",
            CancellationToken.None));
    }

    private static async Task<MemoryStream> CreateImageSharpImageAsync(string contentType)
    {
        IImageEncoder encoder = contentType switch
        {
            "image/jpeg" => new JpegEncoder(),
            "image/png" => new PngEncoder(),
            "image/webp" => new WebpEncoder(),
            "image/gif" => new GifEncoder(),
            _ => throw new ArgumentOutOfRangeException(nameof(contentType)),
        };

        using var image = new Image<Rgba32>(4, 4, new Rgba32(255, 0, 0));
        var stream = new MemoryStream();
        await image.SaveAsync(stream, encoder);
        stream.Position = 0;
        return stream;
    }

    private static MemoryStream CreateHeicSample() => new(Convert.FromBase64String(
        "AAAAHGZ0eXBoZWljAAAAAG1pZjFoZWljbWlhZgAAAaptZXRhAAAAAAAAACFoZGxyAAAAAAAAAABwaWN0AAAAAAAAAAAAAAAAAAAAAA5waXRtAAAAAAACAAAAEGlkYXQAAAAAAAQABAAAADhpbG9jAQAAAERAAAIAAQAAAAAAAAHOAAEAAAAAAAAANAACAAEAAAAAAAAAAQAAAAAAAAAIAAAAOGlpbmYAAAAAAAIAAAAVaW5mZQIAAAEAAQAAaHZjMQAAAAAVaW5mZQIAAAAAAgAAZ3JpZAAAAADVaXBycAAAALNpcGNvAAAAc2h2Y0MBA3AAAAAAAAAAAAAe8AD8/fj4AAAPAyAAAQAYQAEMAf//A3AAAAMAkAAAAwAAAwAeugJAIQABACdCAQEDcAAAAwCQAAADAAADAB6gIIEFluqumubAgAAAAwCAAAADAIQiAAEABkQBwXPBiQAAABRpc3BlAAAAAAAAAEAAAABAAAAAFGlzcGUAAAAAAAAABAAAAAQAAAAQcGl4aQAAAAADCAgIAAAAGmlwbWEAAAAAAAAAAgABAoECAAICA4QAAAAaaXJlZgAAAAAAAAAOZGltZwACAAEAAQAAADxtZGF0AAAAMCgBrxMhZmNA+BD3Z//rvBX/mSE/8zex6c7IR0DA0iCAm0BIk11QCxYQgId2pVbc+A=="));
}
