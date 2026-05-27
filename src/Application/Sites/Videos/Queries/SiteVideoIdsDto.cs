namespace Application.Sites.Videos.Queries;

public sealed record SiteVideoIdsDto(
    Guid VideoId,
    Guid SnapshotId,
    int? DurationSeconds,
    string Category);
