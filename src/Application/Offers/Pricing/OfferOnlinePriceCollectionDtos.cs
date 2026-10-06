using Application.RetailerPriceCollections;

namespace Application.Offers.Pricing;

/// <summary>Identifies one retailer comparison column in an offer collection option.</summary>
public sealed record OfferOnlinePriceCollectionRetailerDto(
    Guid RetailerId,
    string DisplayName,
    bool IsActive);

/// <summary>Explains why one offer product and retailer column cannot be collected.</summary>
public sealed record OfferOnlinePriceCollectionExclusionDto(
    Guid OfferProductLineId,
    Guid ProductId,
    int ProductNumberId,
    string ProductTitle,
    Guid RetailerId,
    string RetailerDisplayName,
    string Code,
    string Message);

/// <summary>Describes one retailer company available to the offer collection picker.</summary>
public sealed record OfferOnlinePriceCollectionCompanyOptionDto(
    Guid CompanyPersonId,
    string CompanyDisplayName,
    IReadOnlyList<OfferOnlinePriceCollectionRetailerDto> Retailers,
    int EligibleListingCount,
    IReadOnlyList<OfferOnlinePriceCollectionExclusionDto> Exclusions,
    bool HasActivePublishedProfile,
    RetailerPriceCollectionRunSummaryDto? UnfinishedRun,
    bool CanStart,
    IReadOnlyList<string> BlockingCodes);

/// <summary>Represents server-derived collection choices for a draft offer.</summary>
public sealed record OfferOnlinePriceCollectionOptionsDto(
    Guid OfferId,
    IReadOnlyList<OfferOnlinePriceCollectionCompanyOptionDto> Companies);

/// <summary>Represents admission of one selected retailer company.</summary>
public sealed record OfferOnlinePriceCollectionStartOutcomeDto(
    Guid CompanyPersonId,
    string CompanyDisplayName,
    string Status,
    string Code,
    string Message,
    RetailerPriceCollectionRunSummaryDto? Run,
    IReadOnlyList<OfferOnlinePriceCollectionExclusionDto> Exclusions);

/// <summary>Represents the independently admitted results of an offer collection request.</summary>
public sealed record OfferOnlinePriceCollectionStartResponseDto(
    Guid OfferId,
    IReadOnlyList<OfferOnlinePriceCollectionStartOutcomeDto> Outcomes);
