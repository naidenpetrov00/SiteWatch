using Domain.SeedWork.Enums;

namespace Application.Sites.Videos.Queries;

public sealed record SiteVideoIdsDto(
    Guid VideoId,
    Guid SnapshotId,
    VideoCategory Category);
