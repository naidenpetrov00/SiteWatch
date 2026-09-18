using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class OfferActivitySection : BaseEntity
{
    private readonly List<OfferProductContribution> _productContributions = [];

    private OfferActivitySection()
    {
    }

    public Guid OfferActivityId { get; private set; }
    public OfferActivity OfferActivity { get; private set; } = null!;
    public Guid SourceSectionId { get; private set; }
    public string? Name { get; private set; }
    public decimal BasisQuantity { get; private set; }
    public ActivityMeasurementUnit MeasurementUnit { get; private set; }
    public decimal RequestedMeasurement { get; private set; }
    public int SortOrder { get; private set; }
    public IReadOnlyCollection<OfferProductContribution> ProductContributions =>
        _productContributions;

    internal static OfferActivitySection Create(
        OfferActivity offerActivity,
        ActivityRequirementSection sourceSection,
        decimal requestedMeasurement)
    {
        ArgumentNullException.ThrowIfNull(offerActivity);
        ArgumentNullException.ThrowIfNull(sourceSection);

        return new OfferActivitySection
        {
            Id = Guid.NewGuid(),
            OfferActivity = offerActivity,
            OfferActivityId = offerActivity.Id,
            SourceSectionId = sourceSection.Id,
            Name = sourceSection.Name,
            BasisQuantity = sourceSection.BasisQuantity,
            MeasurementUnit = sourceSection.MeasurementUnit,
            RequestedMeasurement = OfferQuantity.ValidateInput(
                requestedMeasurement,
                nameof(requestedMeasurement)),
            SortOrder = sourceSection.SortOrder
        };
    }

    internal void UpdateRequestedMeasurement(decimal requestedMeasurement)
    {
        RequestedMeasurement = OfferQuantity.ValidateInput(
            requestedMeasurement,
            nameof(requestedMeasurement));
        foreach (var contribution in _productContributions)
        {
            contribution.Recalculate(RequestedMeasurement, BasisQuantity);
        }
    }

    internal void AddContribution(OfferProductContribution contribution) =>
        _productContributions.Add(contribution);
}
