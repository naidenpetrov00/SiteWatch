using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace Api.Endpoints.Sites;

internal static class MediaUploadValidation
{
    public const long ImageMaxFileSize = 50L * 1024L * 1024L;
    public const long VideoMaxFileSize = 500L * 1024L * 1024L;
    public const long FileMaxFileSize = 100L * 1024L * 1024L;

    public const long ImageMaxRequestSize = ImageMaxFileSize + 1024L * 1024L;
    public const long VideoMaxRequestSize = VideoMaxFileSize + 1024L * 1024L;
    public const long FileMaxRequestSize = FileMaxFileSize + 1024L * 1024L;

    private static readonly IReadOnlySet<string> ImageContentTypes =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp",
            "image/gif",
            "image/heic",
            "image/heif",
        };

    private static readonly IReadOnlySet<string> VideoContentTypes =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "video/mp4",
            "video/quicktime",
            "video/webm",
        };

    public static void ValidateImage(IFormFile file) =>
        Validate(file, ImageMaxFileSize, ImageContentTypes, "image", "JPEG, PNG, WebP, GIF, HEIC, or HEIF images");

    public static void ValidateVideo(IFormFile file) =>
        Validate(file, VideoMaxFileSize, VideoContentTypes, "video", "MP4, MOV, or WebM videos");

    public static void ValidateFile(IFormFile file)
    {
        if (string.IsNullOrWhiteSpace(file.ContentType))
        {
            throw InvalidFile("The file content type is required.");
        }

        ValidateLength(file, FileMaxFileSize, "file");
    }

    private static void Validate(
        IFormFile file,
        long maxFileSize,
        IReadOnlySet<string> allowedContentTypes,
        string kind,
        string allowedDescription)
    {
        ValidateLength(file, maxFileSize, kind);

        if (!allowedContentTypes.Contains(file.ContentType))
        {
            throw InvalidFile($"Only {allowedDescription} are allowed.");
        }
    }

    private static void ValidateLength(IFormFile file, long maxFileSize, string kind)
    {
        if (file.Length <= 0)
        {
            throw InvalidFile($"The {kind} file cannot be empty.");
        }

        if (file.Length > maxFileSize)
        {
            throw InvalidFile($"The {kind} file cannot exceed {maxFileSize / 1024 / 1024} MB.");
        }
    }

    private static ValidationException InvalidFile(string message) =>
        new([new ValidationFailure("file", message)]);
}
