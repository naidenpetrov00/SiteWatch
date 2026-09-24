using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class OfferProductPriceSelection : BaseEntity
{
    private OfferProductPriceSelection()
    {
    }

    public Guid OfferProductLineId { get; private set; }
    public OfferProductLine OfferProductLine { get; private set; } = null!;
    public Guid RetailerId { get; private set; }
    public Guid RetailerListingId { get; private set; }
    public Guid RetailerPriceObservationId { get; private set; }
    public string RetailerDisplayName { get; private set; } = string.Empty;
    public string? ProductUrl { get; private set; }
    public string? RetailerProductCode { get; private set; }
    public decimal Amount { get; private set; }
    public string CurrencyCode { get; private set; } = RetailerPriceObservation.EuroCurrencyCode;
    public PriceBasis Basis { get; private set; }
    public DateTimeOffset ObservedAt { get; private set; }
    public DateTimeOffset RecordedAt { get; private set; }
    public PriceObservationSource Source { get; private set; }
    public string? SourceReference { get; private set; }
    public string RecordedBy { get; private set; } = string.Empty;
    public DateTimeOffset SelectedAt { get; private set; }
    public string SelectedBy { get; private set; } = string.Empty;

    internal static OfferProductPriceSelection Create(
        OfferProductLine productLine,
        OfferRetailerComparison comparison,
        RetailerListing listing,
        RetailerPriceObservation observation,
        DateTimeOffset selectedAt,
        string selectedBy)
    {
        ArgumentNullException.ThrowIfNull(productLine);
        ArgumentNullException.ThrowIfNull(comparison);
        ArgumentNullException.ThrowIfNull(listing);
        ArgumentNullException.ThrowIfNull(observation);
        ArgumentException.ThrowIfNullOrWhiteSpace(selectedBy);

        return new OfferProductPriceSelection
        {
            Id = Guid.NewGuid(),
            OfferProductLine = productLine,
            OfferProductLineId = productLine.Id,
            RetailerId = comparison.RetailerId,
            RetailerListingId = listing.Id,
            RetailerPriceObservationId = observation.Id,
            RetailerDisplayName = comparison.RetailerDisplayName,
            ProductUrl = listing.ProductUrl,
            RetailerProductCode = listing.RetailerProductCode,
            Amount = observation.Amount,
            CurrencyCode = observation.CurrencyCode,
            Basis = observation.Basis,
            ObservedAt = observation.ObservedAt,
            RecordedAt = observation.RecordedAt,
            Source = observation.Source,
            SourceReference = observation.SourceReference,
            RecordedBy = observation.RecordedBy,
            SelectedAt = selectedAt,
            SelectedBy = selectedBy
        };
    }
}
