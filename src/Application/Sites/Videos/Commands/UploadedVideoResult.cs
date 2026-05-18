namespace Application.Sites.Videos.Commands;

public sealed record UploadedVideoResult(
    Guid VideoFileId,
    Guid SnapshotFileId,
    int? DurationSeconds);
