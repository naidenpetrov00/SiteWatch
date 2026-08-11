using Api.Endpoints.Sites;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Api.Tests.Endpoints.Sites;

public sealed class MediaUploadValidationTests
{
    [Theory]
    [InlineData(" IMAGE/JPG ", "image/jpeg")]
    [InlineData("IMAGE/PNG", "image/png")]
    [InlineData(" image/heif ", "image/heif")]
    public void ValidateImage_returns_the_canonical_allowed_content_type(
        string declaredContentType,
        string expectedContentType)
    {
        var file = CreateFile(declaredContentType);

        var contentType = MediaUploadValidation.ValidateImage(file);

        Assert.Equal(expectedContentType, contentType);
    }

    [Theory]
    [InlineData(" VIDEO/MP4 ", "video/mp4")]
    [InlineData("Video/QuickTime", "video/quicktime")]
    [InlineData("video/webm", "video/webm")]
    public void ValidateVideo_returns_the_canonical_allowed_content_type(
        string declaredContentType,
        string expectedContentType)
    {
        var file = CreateFile(declaredContentType);

        var contentType = MediaUploadValidation.ValidateVideo(file);

        Assert.Equal(expectedContentType, contentType);
    }

    [Theory]
    [InlineData("text/plain")]
    [InlineData("application/octet-stream")]
    public void ValidateImage_rejects_disallowed_declared_content_types(string contentType)
    {
        var file = CreateFile(contentType);

        var exception = Assert.Throws<ValidationException>(() =>
            MediaUploadValidation.ValidateImage(file));

        Assert.Contains(exception.Errors, error => error.PropertyName == "file");
    }

    [Fact]
    public void ValidateVideo_rejects_empty_files()
    {
        var file = CreateFile("video/mp4", []);

        var exception = Assert.Throws<ValidationException>(() =>
            MediaUploadValidation.ValidateVideo(file));

        Assert.Contains(exception.Errors, error => error.ErrorMessage.Contains("empty"));
    }

    private static FormFile CreateFile(string contentType, byte[]? content = null)
    {
        content ??= [1];
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "file", "upload")
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType,
        };
    }
}
