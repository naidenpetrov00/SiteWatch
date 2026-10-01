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
    long ConfigurationRevision,
    long? ValidatedConfigurationRevision,
    bool IsCurrentConfigurationValidated,
    DateTimeOffset? LastSuccessfulTestAt,
    Guid? LastSuccessfulTestRuleId,
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

/// <summary>Requests a draft test using one retailer listing or one manual URL.</summary>
public sealed record TestRetailerExtractionProfileRequest
{
    public string SourceType { get; init; } = string.Empty;
    public Guid? RetailerListingId { get; init; }
    public string? ManualUrl { get; init; }
}

/// <summary>Describes one enabled extraction rule attempted during a draft test.</summary>
public sealed record RetailerExtractionRuleDiagnosticDto(
    Guid RuleId,
    string RuleName,
    string RuleType,
    int Priority,
    string Outcome,
    string Explanation);

/// <summary>Describes the first successful extraction in a draft test.</summary>
public sealed record RetailerExtractionSuccessDto(
    decimal Amount,
    string CurrencyCode,
    string PriceBasis,
    Guid ProfileId,
    Guid RuleId,
    string RuleName,
    int RulePriority,
    string RawValue);

/// <summary>Represents a completed synchronous draft extraction test.</summary>
public sealed record RetailerExtractionTestResultDto(
    DateTimeOffset TestedAt,
    string TestedUrl,
    Guid ProfileId,
    bool Success,
    bool IsCurrentConfigurationValidated,
    RetailerExtractionSuccessDto? Extraction,
    IReadOnlyList<RetailerExtractionRuleDiagnosticDto> Diagnostics);
