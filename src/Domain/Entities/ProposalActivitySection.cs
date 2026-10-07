using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class ProposalActivitySection : BaseEntity
{
    private ProposalActivitySection()
    {
    }

    public Guid ProposalActivityId { get; private set; }
    public ProposalActivity ProposalActivity { get; private set; } = null!;
    public Guid SourceOfferActivitySectionId { get; private set; }
    public string? Name { get; private set; }
    public decimal BasisQuantity { get; private set; }
    public ActivityMeasurementUnit MeasurementUnit { get; private set; }
    public decimal RequestedMeasurement { get; private set; }
    public ActivityPricingMode PricingMode { get; private set; }
    public decimal? PriceAmount { get; private set; }
    public decimal CalculatedTotal { get; private set; }
    public int SortOrder { get; private set; }

    internal static ProposalActivitySection Create(
        ProposalActivity activity,
        OfferActivitySection source)
    {
        if (!source.PricingMode.HasValue)
        {
            throw new InvalidOperationException(
                "Every activity section requires valid pricing before creating a Proposal.");
        }

        var total = source.CalculatePriceTotal()
            ?? throw new InvalidOperationException(
                "Every activity section requires a calculable total before creating a Proposal.");

        return new ProposalActivitySection
        {
            Id = Guid.NewGuid(),
            ProposalActivity = activity,
            ProposalActivityId = activity.Id,
            SourceOfferActivitySectionId = source.Id,
            Name = source.Name,
            BasisQuantity = source.BasisQuantity,
            MeasurementUnit = source.MeasurementUnit,
            RequestedMeasurement = source.RequestedMeasurement,
            PricingMode = source.PricingMode.Value,
            PriceAmount = source.PriceAmount,
            CalculatedTotal = total,
            SortOrder = source.SortOrder
        };
    }
}
