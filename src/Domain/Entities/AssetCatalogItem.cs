using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class AssetCatalogItem : BaseAuditableEntity, IAgregateRoot
{
    public const int MaxNameLength = 200;
    public const int MaxManufacturerLength = 100;
    public const int MaxModelLength = 100;
    public const int MaxDescriptionLength = 2000;
    public const int MaxRejectionReasonLength = 2000;

    private readonly HashSet<Asset> _assets = [];

    private AssetCatalogItem()
    {
    }

    public AssetCatalogClassification Classification { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Manufacturer { get; private set; }
    public string? Model { get; private set; }
    public string? Description { get; private set; }
    public AssetReviewStatus ReviewStatus { get; private set; }
    public string? ReviewedById { get; private set; }
    public ApplicationUser? ReviewedBy { get; private set; }
    public DateTimeOffset? ReviewedAt { get; private set; }
    public string? RejectionReason { get; private set; }
    public byte[] RowVersion { get; private set; } = [];
    public IReadOnlyCollection<Asset> Assets => _assets;

    public static AssetCatalogItem CreateActive(
        AssetCatalogClassification classification,
        string name,
        string? manufacturer,
        string? model,
        string? description,
        ApplicationUser reviewer,
        DateTimeOffset reviewedAt)
    {
        var item = CreatePendingReview(classification, name, manufacturer, model, description);
        item.Activate(reviewer, reviewedAt);
        return item;
    }

    public static AssetCatalogItem CreatePendingReview(
        AssetCatalogClassification classification,
        string name,
        string? manufacturer,
        string? model,
        string? description)
    {
        EnsureDefined(classification, nameof(classification));

        var item = new AssetCatalogItem
        {
            Id = Guid.NewGuid(),
            ReviewStatus = AssetReviewStatus.PendingReview
        };
        item.UpdateDetails(classification, name, manufacturer, model, description);
        return item;
    }

    public void UpdateDetails(
        AssetCatalogClassification classification,
        string name,
        string? manufacturer,
        string? model,
        string? description)
    {
        EnsureNotRejected();
        EnsureDefined(classification, nameof(classification));

        var normalizedName = NormalizeRequired(name, MaxNameLength, nameof(name));
        var normalizedManufacturer = NormalizeOptional(
            manufacturer,
            MaxManufacturerLength,
            nameof(manufacturer));
        var normalizedModel = NormalizeOptional(model, MaxModelLength, nameof(model));
        var normalizedDescription = NormalizeOptional(
            description,
            MaxDescriptionLength,
            nameof(description),
            false);

        Classification = classification;
        Name = normalizedName;
        Manufacturer = normalizedManufacturer;
        Model = normalizedModel;
        Description = normalizedDescription;
    }

    public void Activate(ApplicationUser reviewer, DateTimeOffset reviewedAt)
    {
        EnsurePendingReview();
        SetReview(reviewer, reviewedAt, null);
        ReviewStatus = AssetReviewStatus.Active;
    }

    public void Reject(
        ApplicationUser reviewer,
        DateTimeOffset reviewedAt,
        string? rejectionReason)
    {
        EnsurePendingReview();
        SetReview(
            reviewer,
            reviewedAt,
            NormalizeOptional(
                rejectionReason,
                MaxRejectionReasonLength,
                nameof(rejectionReason),
                false));
        ReviewStatus = AssetReviewStatus.Rejected;
    }

    private void SetReview(
        ApplicationUser reviewer,
        DateTimeOffset reviewedAt,
        string? rejectionReason)
    {
        ArgumentNullException.ThrowIfNull(reviewer);
        ArgumentException.ThrowIfNullOrWhiteSpace(reviewer.Id);
        EnsureUtc(reviewedAt, nameof(reviewedAt));

        ReviewedBy = reviewer;
        ReviewedById = reviewer.Id;
        ReviewedAt = reviewedAt;
        RejectionReason = rejectionReason;
    }

    private void EnsurePendingReview()
    {
        if (ReviewStatus != AssetReviewStatus.PendingReview)
        {
            throw new InvalidOperationException("Only a pending catalog item can be reviewed.");
        }
    }

    private void EnsureNotRejected()
    {
        if (ReviewStatus == AssetReviewStatus.Rejected)
        {
            throw new InvalidOperationException("A rejected catalog item cannot be changed.");
        }
    }

    private static void EnsureDefined<TEnum>(TEnum value, string parameterName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Unsupported value.");
        }
    }

    private static void EnsureUtc(DateTimeOffset value, string parameterName)
    {
        if (value.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("The timestamp must be UTC.", parameterName);
        }
    }

    private static string NormalizeRequired(string value, int maxLength, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return Normalize(value, maxLength, parameterName, true)!;
    }

    private static string? NormalizeOptional(
        string? value,
        int maxLength,
        string parameterName,
        bool collapseWhitespace = true) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : Normalize(value, maxLength, parameterName, collapseWhitespace);

    private static string Normalize(
        string value,
        int maxLength,
        string parameterName,
        bool collapseWhitespace)
    {
        var normalized = collapseWhitespace
            ? string.Join(" ", value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            : value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                normalized.Length,
                $"The value cannot exceed {maxLength} characters.");
        }

        return normalized;
    }
}
