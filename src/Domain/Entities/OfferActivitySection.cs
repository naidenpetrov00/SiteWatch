using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class OfferActivitySection : BaseEntity
{
    public const decimal MaximumPriceAmount = 9999999999999999.99m;

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
    public ActivityPricingMode? PricingMode { get; private set; }
    public decimal? PriceAmount { get; private set; }
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

    internal void UpdatePricing(ActivityPricingMode? pricingMode, decimal? priceAmount)
    {
        if (!pricingMode.HasValue)
        {
            if (priceAmount.HasValue)
            {
                throw new ArgumentException(
                    "Missing activity pricing cannot include an amount.",
                    nameof(priceAmount));
            }

            PricingMode = null;
            PriceAmount = null;
            return;
        }

        if (!Enum.IsDefined(pricingMode.Value))
        {
            throw new ArgumentOutOfRangeException(nameof(pricingMode));
        }

        if (pricingMode == ActivityPricingMode.Free)
        {
            if (priceAmount.HasValue)
            {
                throw new ArgumentException(
                    "Free activity pricing cannot include an amount.",
                    nameof(priceAmount));
            }

            PricingMode = pricingMode;
            PriceAmount = null;
            return;
        }

        if (!priceAmount.HasValue
            || priceAmount.Value <= 0m
            || priceAmount.Value > MaximumPriceAmount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(priceAmount),
                priceAmount,
                "An activity price must be within the supported positive EUR range.");
        }

        if (decimal.Round(priceAmount.Value, 2) != priceAmount.Value)
        {
            throw new ArgumentException(
                "An activity price cannot have more than two decimal places.",
                nameof(priceAmount));
        }

        PricingMode = pricingMode;
        PriceAmount = priceAmount;
    }

    public decimal? CalculatePriceTotal() => PricingMode switch
    {
        null => null,
        ActivityPricingMode.Free => 0m,
        ActivityPricingMode.Fixed => PriceAmount,
        ActivityPricingMode.PerMeasurement => decimal.Round(
            RequestedMeasurement * PriceAmount!.Value,
            2,
            MidpointRounding.AwayFromZero),
        _ => throw new InvalidOperationException("Unsupported activity pricing mode.")
    };

    internal void AddContribution(OfferProductContribution contribution) =>
        _productContributions.Add(contribution);
}
