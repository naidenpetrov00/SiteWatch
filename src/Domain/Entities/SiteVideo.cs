using Domain.SeedWork;
using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class SiteVideo : BaseAuditableEntity
{
    private SiteVideo()
    {
    }

    public SiteVideo(
        Guid siteId,
        Guid videoId,
        Guid snapshotId,
        int? durationSeconds,
        string category)
    {
        SiteId = siteId;
        VideoId = videoId;
        SnapshotId = snapshotId;
        DurationSeconds = durationSeconds;
        Category = ValidateCategory(category);
        Created = DateTimeOffset.UtcNow;
    }

    public Guid SiteId { get; private set; }
    public Guid VideoId { get; private set; }
    public Guid SnapshotId { get; private set; }
    public int? DurationSeconds { get; private set; }
    public string Category { get; private set; } = string.Empty;

    public Site Site { get; private set; } = null!;

    public void ChangeCategory(string category) => Category = ValidateCategory(category);

    private static string ValidateCategory(string category)
    {
        var normalizedCategory = SiteMediaPolicy.NormalizeCategory(category)
            ?? throw new ArgumentException("Media category cannot be empty.", nameof(category));

        return normalizedCategory.Length <= SiteMediaPolicy.MaxCategoryLength
            ? normalizedCategory
            : throw new ArgumentException(
                $"Media category cannot exceed {SiteMediaPolicy.MaxCategoryLength} characters.",
                nameof(category));
    }
}
