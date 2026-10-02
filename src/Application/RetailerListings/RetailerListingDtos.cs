namespace Application.RetailerListings;

/// <summary>Represents one immutable retailer price observation.</summary>
public sealed record RetailerPriceObservationDto(
    Guid Id,
    decimal Amount,
    string CurrencyCode,
    string Basis,
    DateTimeOffset ObservedAt,
    DateTimeOffset RecordedAt,
    string Source,
    string? SourceReference,
    string RecordedBy);

/// <summary>Represents a Product–Retailer listing and its latest known price.</summary>
public sealed record RetailerListingDto(
    Guid Id,
    Guid ProductId,
    int ProductNumberId,
    string ProductTitle,
    string ProductStatus,
    string? ProductBrand,
    string? ProductModel,
    decimal? ProductPackageQuantity,
    string? ProductPackageUnit,
    Guid RetailerId,
    string RetailerDisplayName,
    string RetailerBaseWebsiteUrl,
    bool RetailerIsActive,
    bool IsActive,
    string? ProductUrl,
    string? RetailerProductCode,
    DateTimeOffset Created,
    DateTimeOffset LastModified,
    RetailerPriceObservationDto? LatestObservation);

/// <summary>Represents paged price history and listing metadata.</summary>
public sealed record RetailerPriceHistoryDto(
    Guid ProductId,
    Guid RetailerId,
    Guid? RetailerListingId,
    bool? RetailerListingIsActive,
    string? ProductUrl,
    string? RetailerProductCode,
    IReadOnlyList<RetailerPriceObservationDto> Items,
    int PageIndex,
    int PageSize,
    int TotalCount);
