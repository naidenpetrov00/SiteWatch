using Domain.SeedWork;
using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class SiteImage : BaseAuditableEntity
{
    private SiteImage()
    {
    }

    public SiteImage(Guid siteId, Guid imageId, Guid thumbnailImageId, string category)
    {
        SiteId = siteId;
        ImageId = imageId;
        ThumbnailImageId = thumbnailImageId;
        Category = ValidateCategory(category);
        Created = DateTimeOffset.UtcNow;
    }

    public Guid SiteId { get; private set; }
    public Guid ImageId { get; private set; }
    public Guid ThumbnailImageId { get; private set; }
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
