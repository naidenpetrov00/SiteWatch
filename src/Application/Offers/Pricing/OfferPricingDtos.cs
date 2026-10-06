using Application.RetailerListings;

namespace Application.Offers.Pricing;

/// <summary>Represents the complete retailer comparison matrix for an Offer.</summary>
public sealed record OfferPricingMatrixDto(
    Guid OfferId,
    string Status,
    string CurrencyCode,
    bool RequiredPricingComplete,
    bool OptionalPricingComplete,
    decimal? RequiredTotal,
    decimal? OptionalTotal,
    OfferCommercialTotalsDto CommercialTotals,
    IReadOnlyList<OfferPricingRetailerDto> Retailers,
    IReadOnlyList<OfferPricingProductRowDto> Products);

/// <summary>Represents a retailer selected for comparison by an Offer.</summary>
public sealed record OfferPricingRetailerDto(
    Guid RetailerId,
    string DisplayName,
    string BaseWebsiteUrl,
    bool IsActive);

/// <summary>Represents one Product row in an Offer pricing matrix.</summary>
public sealed record OfferPricingProductRowDto(
    Guid OfferProductLineId,
    Guid ProductId,
    int ProductNumberId,
    string Title,
    string Category,
    string? Brand,
    string? Model,
    decimal? PackageQuantity,
    string? PackageUnit,
    decimal RequiredQuantity,
    decimal OptionalQuantity,
    IReadOnlyList<OfferPricingRequirementDto> Requirements,
    decimal? SelectedRequiredTotal,
    decimal? SelectedOptionalTotal,
    OfferSelectedPriceDto? SelectedPrice,
    IReadOnlyList<OfferRetailerPriceCellDto> RetailerPrices);

/// <summary>Explains one Activity requirement contributing to an aggregate Product row.</summary>
public sealed record OfferPricingRequirementDto(
    Guid Id,
    Guid OfferActivityId,
    int ActivityNumberId,
    string ActivityName,
    int ActivitySortOrder,
    Guid OfferActivitySectionId,
    string? SectionName,
    int SectionSortOrder,
    decimal BasisQuantity,
    string MeasurementUnit,
    decimal RequestedMeasurement,
    decimal ConfiguredQuantity,
    bool IsRequired,
    string QuantityBehavior,
    string? Notes,
    int SortOrder,
    decimal CalculatedQuantity);

/// <summary>Represents the latest reusable price for one Product and Retailer.</summary>
public sealed record OfferRetailerPriceCellDto(
    Guid OfferProductLineId,
    Guid ProductId,
    Guid RetailerId,
    Guid? RetailerListingId,
    bool? RetailerListingIsActive,
    string? ProductUrl,
    string? RetailerProductCode,
    RetailerPriceObservationDto? LatestObservation,
    decimal? ComparableRequiredTotal,
    decimal? ComparableOptionalTotal,
    bool IsCheapest);


/// <summary>Represents an Offer-owned snapshot of an explicitly selected price.</summary>
public sealed record OfferSelectedPriceDto(
    Guid RetailerId,
    Guid RetailerListingId,
    Guid RetailerPriceObservationId,
    string RetailerDisplayName,
    string? ProductUrl,
    string? RetailerProductCode,
    decimal Amount,
    string CurrencyCode,
    string Basis,
    DateTimeOffset ObservedAt,
    DateTimeOffset RecordedAt,
    string Source,
    string? SourceReference,
    string RecordedBy,
    DateTimeOffset SelectedAt,
    string SelectedBy,
    bool HasNewerObservation);
