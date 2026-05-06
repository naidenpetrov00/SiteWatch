using Domain.SeedWork;

namespace Domain.Entities;

public sealed class SiteImage : BaseAuditableEntity
{
    private SiteImage()
    {
    }

    public SiteImage(Guid siteId, Guid imageId, Guid thumbnailImageId)
    {
        SiteId = siteId;
        ImageId = imageId;
        ThumbnailImageId = thumbnailImageId;
    }

    public Guid SiteId { get; private set; }
    public Guid ImageId { get; private set; }
    public Guid ThumbnailImageId { get; private set; }

    public Site Site { get; private set; } = null!;
}