namespace Application.Sites.Images.Commands;

public sealed record UploadedImageResult(
    Guid OriginalFileId,
    Guid ThumbnailFileId);