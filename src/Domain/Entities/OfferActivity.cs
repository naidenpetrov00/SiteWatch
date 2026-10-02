using Domain.SeedWork;

namespace Domain.Entities;

public sealed class OfferActivity : BaseEntity
{
    private readonly List<OfferActivitySection> _sections = [];

    private OfferActivity()
    {
    }

    public Guid OfferId { get; private set; }
    public Offer Offer { get; private set; } = null!;
    public Guid SourceActivityId { get; private set; }
    public Activity SourceActivity { get; private set; } = null!;
    public int ActivityNumberId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int SortOrder { get; private set; }
    public IReadOnlyCollection<OfferActivitySection> Sections => _sections;

    public static OfferActivity Create(Offer offer, Activity sourceActivity, int sortOrder)
    {
        ArgumentNullException.ThrowIfNull(offer);
        ArgumentNullException.ThrowIfNull(sourceActivity);
        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortOrder));
        }

        return new OfferActivity
        {
            Id = Guid.NewGuid(),
            Offer = offer,
            OfferId = offer.Id,
            SourceActivity = sourceActivity,
            SourceActivityId = sourceActivity.Id,
            ActivityNumberId = sourceActivity.NumberId,
            Name = sourceActivity.Name,
            Description = sourceActivity.Description,
            SortOrder = sortOrder
        };
    }

    public OfferActivitySection AddSection(
        ActivityRequirementSection sourceSection,
        decimal requestedMeasurement)
    {
        if (sourceSection.ActivityId != SourceActivityId)
        {
            throw new InvalidOperationException("The requirement section belongs to another activity.");
        }

        var section = OfferActivitySection.Create(this, sourceSection, requestedMeasurement);
        _sections.Add(section);
        return section;
    }
}
