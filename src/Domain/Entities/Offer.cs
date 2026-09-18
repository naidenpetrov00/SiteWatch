using Ardalis.GuardClauses;
using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class Offer : BaseAuditableEntity, IHasNumberId, IAgregateRoot
{
    public const int MaxTitleLength = 200;
    public const int MaxNotesLength = 2000;

    private Offer()
    {
    }

    private readonly List<OfferActivity> _activities = [];
    private readonly List<OfferProductLine> _productLines = [];

    public int NumberId { get; private set; }
    public Guid SiteId { get; private set; }
    public Site Site { get; private set; } = null!;
    public string? Title { get; private set; }
    public string? Notes { get; private set; }
    public OfferStatus Status { get; private set; }
    public IReadOnlyCollection<OfferActivity> Activities => _activities;
    public IReadOnlyCollection<OfferProductLine> ProductLines => _productLines;

    public static Offer Create(Site site)
    {
        var normalizedSite = Guard.Against.Null(site);

        return new Offer
        {
            Id = Guid.NewGuid(),
            SiteId = normalizedSite.Id,
            Site = normalizedSite,
            Status = OfferStatus.Draft
        };
    }

    public void UpdateMetadata(string? title, string? notes)
    {
        EnsureDraft();
        Title = NormalizeOptional(title, MaxTitleLength, nameof(title));
        Notes = NormalizeOptional(notes, MaxNotesLength, nameof(notes), collapseWhitespace: false);
    }

    public bool Archive()
    {
        if (Status == OfferStatus.Archived)
        {
            return false;
        }

        Status = OfferStatus.Archived;
        return true;
    }

    public void AddActivity(OfferActivity activity)
    {
        EnsureDraft();
        Guard.Against.Null(activity);
        if (activity.OfferId != Id)
        {
            throw new InvalidOperationException("The activity snapshot belongs to another offer.");
        }

        if (_activities.Any(current => current.SourceActivityId == activity.SourceActivityId))
        {
            throw new InvalidOperationException("The activity is already included in this offer.");
        }

        _activities.Add(activity);
    }

    public void RemoveActivity(OfferActivity activity)
    {
        EnsureDraft();
        Guard.Against.Null(activity);
        if (!_activities.Remove(activity))
        {
            throw new InvalidOperationException("The activity snapshot does not belong to this offer.");
        }
    }

    public void UpdateActivityMeasurements(
        OfferActivity activity,
        IReadOnlyDictionary<Guid, decimal> measurements)
    {
        EnsureDraft();
        Guard.Against.Null(activity);
        Guard.Against.Null(measurements);
        if (!_activities.Contains(activity))
        {
            throw new InvalidOperationException("The activity snapshot does not belong to this offer.");
        }

        foreach (var section in activity.Sections)
        {
            if (!measurements.TryGetValue(section.Id, out var requestedMeasurement))
            {
                throw new InvalidOperationException(
                    "Every activity section requires a requested measurement.");
            }

            section.UpdateRequestedMeasurement(requestedMeasurement);
        }
    }

    public void AddProductLine(OfferProductLine productLine)
    {
        EnsureDraft();
        Guard.Against.Null(productLine);
        if (productLine.OfferId != Id)
        {
            throw new InvalidOperationException("The product line belongs to another offer.");
        }

        if (_productLines.Any(current => current.ProductId == productLine.ProductId))
        {
            throw new InvalidOperationException("The product already has an aggregate offer line.");
        }

        _productLines.Add(productLine);
    }

    public void RemoveProductLine(OfferProductLine productLine)
    {
        EnsureDraft();
        Guard.Against.Null(productLine);
        if (!_productLines.Remove(productLine))
        {
            throw new InvalidOperationException("The product line does not belong to this offer.");
        }
    }

    public void EnsureDraft()
    {
        if (Status != OfferStatus.Draft)
        {
            throw new InvalidOperationException("Only draft offers can be edited.");
        }
    }

    private static string? NormalizeOptional(
        string? value,
        int maxLength,
        string parameterName,
        bool collapseWhitespace = true)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = collapseWhitespace
            ? string.Join(" ", value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            : value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                normalized.Length,
                $"The value must be at most {maxLength} characters.");
        }

        return normalized;
    }
}
