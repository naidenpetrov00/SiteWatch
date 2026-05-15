using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class SiteImage : BaseAuditableEntity
{
    private SiteImage()
    {
    }

    public SiteImage(Guid siteId, Guid imageId, Guid thumbnailImageId, ImageCategory category = ImageCategory.Other)
    {
        SiteId = siteId;
        ImageId = imageId;
        ThumbnailImageId = thumbnailImageId;
        Category = category;
    }

    public Guid SiteId { get; private set; }
    public Guid ImageId { get; private set; }
    public Guid ThumbnailImageId { get; private set; }
    public ImageCategory Category { get; private set; }

    public Site Site { get; private set; } = null!;

    public void ChangeCategory(ImageCategory category) => Category = category;
}