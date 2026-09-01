using Domain.SeedWork.Enums;

namespace Application.Issues.Attachments;

public static class IssueAttachmentValidation
{
    public const long ImageMaxFileSize = 50L * 1024L * 1024L;
    public const long VideoMaxFileSize = 500L * 1024L * 1024L;
    public const long FileMaxFileSize = 100L * 1024L * 1024L;
    public const long MaxRequestSize = VideoMaxFileSize + 1024L * 1024L;
    public const int MaxFileNameLength = 512;
    public const int MaxContentTypeLength = 128;

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

    public static string NormalizeContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return "application/octet-stream";
        }

        var normalized = contentType.Trim().ToLowerInvariant();
        return normalized == "image/jpg" ? "image/jpeg" : normalized;
    }

    public static IssueAttachmentKind GetKind(string contentType)
    {
        var normalized = NormalizeContentType(contentType);
        if (ImageContentTypes.Contains(normalized))
        {
            return IssueAttachmentKind.Image;
        }

        if (VideoContentTypes.Contains(normalized))
        {
            return IssueAttachmentKind.Video;
        }

        if (normalized.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Only JPEG, PNG, WebP, GIF, HEIC, or HEIF images are allowed.",
                nameof(contentType));
        }

        if (normalized.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Only MP4, MOV, or WebM videos are allowed.",
                nameof(contentType));
        }

        return IssueAttachmentKind.File;
    }

    public static long GetMaxFileSize(IssueAttachmentKind kind) => kind switch
    {
        IssueAttachmentKind.Image => ImageMaxFileSize,
        IssueAttachmentKind.Video => VideoMaxFileSize,
        _ => FileMaxFileSize,
    };
}
