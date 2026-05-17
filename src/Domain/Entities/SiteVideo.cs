using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class SiteVideo : BaseAuditableEntity
{
    private SiteVideo()
    {
    }

    public SiteVideo(Guid siteId, Guid videoId, Guid snapshotId, VideoCategory category = VideoCategory.Other)
    {
        SiteId = siteId;
        VideoId = videoId;
        SnapshotId = snapshotId;
        Category = category;
    }

    public Guid SiteId { get; private set; }
    public Guid VideoId { get; private set; }
    public Guid SnapshotId { get; private set; }
    public VideoCategory Category { get; private set; }

    public Site Site { get; private set; } = null!;

    public void ChangeCategory(VideoCategory category) => Category = category;
}
