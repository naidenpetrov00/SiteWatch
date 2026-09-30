namespace Application.RetailerExtractionProfiles;

/// <summary>Represents one extraction-profile version in retailer history.</summary>
public sealed record RetailerExtractionProfileSummaryDto(
    Guid Id,
    Guid RetailerId,
    int Version,
    string Status,
    bool IsActive,
    DateTimeOffset Created,
    DateTimeOffset LastModified,
    DateTimeOffset? PublishedAt,
    string? PublishedBy,
    int RuleCount,
    int EnabledRuleCount);

/// <summary>Represents one ordered extraction rule.</summary>
public sealed record RetailerExtractionRuleDto(
    Guid Id,
    string Name,
    bool IsEnabled,
    int Priority,
    string RuleType,
    string? JsonLdObjectType,
    string? JsonLdPricePath,
    string? JsonLdCurrencyPath,
    string? CssSelector,
    string? CssValueSource,
    string? CssAttributeName,
    string DecimalSeparator,
    string ThousandsSeparator,
    string ExpectedCurrencyCode,
    string PriceBasis,
    decimal? MinimumValue,
    decimal? MaximumValue);

/// <summary>Represents a complete retailer extraction-profile version.</summary>
public sealed record RetailerExtractionProfileDetailsDto(
    RetailerExtractionProfileSummaryDto Summary,
    IReadOnlyList<string> AllowedHosts,
    IReadOnlyList<RetailerExtractionRuleDto> Rules);

/// <summary>Represents the current editable and active profiles for a retailer.</summary>
public sealed record RetailerExtractionCurrentProfilesDto(
    RetailerExtractionProfileDetailsDto? Draft,
    RetailerExtractionProfileDetailsDto? Active);

/// <summary>Common configuration fields for creating or updating an extraction rule.</summary>
public abstract record RetailerExtractionRuleRequest
{
    public string Name { get; init; } = string.Empty;
    public bool IsEnabled { get; init; } = true;
    public string RuleType { get; init; } = string.Empty;
    public string? JsonLdObjectType { get; init; }
    public string? JsonLdPricePath { get; init; }
    public string? JsonLdCurrencyPath { get; init; }
    public string? CssSelector { get; init; }
    public string? CssValueSource { get; init; }
    public string? CssAttributeName { get; init; }
    public string DecimalSeparator { get; init; } = "comma";
    public string ThousandsSeparator { get; init; } = "space";
    public string ExpectedCurrencyCode { get; init; } = "EUR";
    public string PriceBasis { get; init; } = "item";
    public decimal? MinimumValue { get; init; }
    public decimal? MaximumValue { get; init; }
}
