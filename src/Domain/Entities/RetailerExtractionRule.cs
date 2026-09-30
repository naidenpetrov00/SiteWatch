using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed record RetailerExtractionRuleConfiguration(
    string Name,
    bool IsEnabled,
    RetailerExtractionRuleType RuleType,
    string? JsonLdObjectType,
    string? JsonLdPricePath,
    string? JsonLdCurrencyPath,
    string? CssSelector,
    RetailerExtractionCssValueSource? CssValueSource,
    string? CssAttributeName,
    RetailerExtractionDecimalSeparator DecimalSeparator,
    RetailerExtractionThousandsSeparator ThousandsSeparator,
    string ExpectedCurrencyCode,
    PriceBasis PriceBasis,
    decimal? MinimumValue,
    decimal? MaximumValue);

public sealed class RetailerExtractionRule : BaseEntity
{
    public const string EuroCurrencyCode = "EUR";
    public const int MaxNameLength = 200;
    public const int MaxObjectTypeLength = 200;
    public const int MaxPathLength = 512;
    public const int MaxSelectorLength = 2048;
    public const int MaxAttributeNameLength = 200;
    public const decimal MaximumAcceptedValue = 9999999999999999.99m;

    private RetailerExtractionRule()
    {
    }

    public Guid ExtractionProfileId { get; private set; }
    public RetailerExtractionProfile ExtractionProfile { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; }
    public int Priority { get; private set; }
    public RetailerExtractionRuleType RuleType { get; private set; }
    public string? JsonLdObjectType { get; private set; }
    public string? JsonLdPricePath { get; private set; }
    public string? JsonLdCurrencyPath { get; private set; }
    public string? CssSelector { get; private set; }
    public RetailerExtractionCssValueSource? CssValueSource { get; private set; }
    public string? CssAttributeName { get; private set; }
    public RetailerExtractionDecimalSeparator DecimalSeparator { get; private set; }
    public RetailerExtractionThousandsSeparator ThousandsSeparator { get; private set; }
    public string ExpectedCurrencyCode { get; private set; } = EuroCurrencyCode;
    public PriceBasis PriceBasis { get; private set; }
    public decimal? MinimumValue { get; private set; }
    public decimal? MaximumValue { get; private set; }

    internal static RetailerExtractionRule Create(
        RetailerExtractionProfile profile,
        int priority,
        RetailerExtractionRuleConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(profile);
        if (priority <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(priority));
        }

        var rule = new RetailerExtractionRule
        {
            Id = Guid.NewGuid(),
            ExtractionProfile = profile,
            ExtractionProfileId = profile.Id,
            Priority = priority
        };
        rule.Apply(configuration);
        return rule;
    }

    internal RetailerExtractionRule CloneFor(
        RetailerExtractionProfile profile,
        int priority) =>
        Create(
            profile,
            priority,
            new RetailerExtractionRuleConfiguration(
                Name,
                IsEnabled,
                RuleType,
                JsonLdObjectType,
                JsonLdPricePath,
                JsonLdCurrencyPath,
                CssSelector,
                CssValueSource,
                CssAttributeName,
                DecimalSeparator,
                ThousandsSeparator,
                ExpectedCurrencyCode,
                PriceBasis,
                MinimumValue,
                MaximumValue));

    internal void Update(RetailerExtractionRuleConfiguration configuration) =>
        Apply(configuration);

    internal void SetEnabled(bool isEnabled) => IsEnabled = isEnabled;

    internal void SetPriority(int priority)
    {
        if (priority <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(priority));
        }

        Priority = priority;
    }

    private void Apply(RetailerExtractionRuleConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var normalizedName = NormalizeRequired(
            configuration.Name,
            MaxNameLength,
            nameof(configuration.Name));
        if (!normalizedName.Any(char.IsLetterOrDigit))
        {
            throw new ArgumentException("Rule names must contain a letter or digit.");
        }

        if (!Enum.IsDefined(configuration.RuleType)
            || !Enum.IsDefined(configuration.DecimalSeparator)
            || !Enum.IsDefined(configuration.ThousandsSeparator)
            || !Enum.IsDefined(configuration.PriceBasis))
        {
            throw new ArgumentOutOfRangeException(nameof(configuration));
        }

        if (!string.Equals(
            configuration.ExpectedCurrencyCode?.Trim(),
            EuroCurrencyCode,
            StringComparison.Ordinal))
        {
            throw new ArgumentException("Only EUR currency expectation is supported.");
        }

        if ((configuration.DecimalSeparator == RetailerExtractionDecimalSeparator.Dot
                && configuration.ThousandsSeparator == RetailerExtractionThousandsSeparator.Dot)
            || (configuration.DecimalSeparator == RetailerExtractionDecimalSeparator.Comma
                && configuration.ThousandsSeparator == RetailerExtractionThousandsSeparator.Comma))
        {
            throw new ArgumentException("Decimal and thousands separators must differ.");
        }

        ValidateRange(configuration.MinimumValue, configuration.MaximumValue);

        string? jsonLdObjectType = null;
        string? jsonLdPricePath = null;
        string? jsonLdCurrencyPath = null;
        string? cssSelector = null;
        RetailerExtractionCssValueSource? cssValueSource = null;
        string? cssAttributeName = null;

        if (configuration.RuleType == RetailerExtractionRuleType.JsonLd)
        {
            jsonLdObjectType = NormalizeRequired(
                configuration.JsonLdObjectType,
                MaxObjectTypeLength,
                nameof(configuration.JsonLdObjectType));
            jsonLdPricePath = NormalizeRequired(
                configuration.JsonLdPricePath,
                MaxPathLength,
                nameof(configuration.JsonLdPricePath));
            jsonLdCurrencyPath = NormalizeOptional(
                configuration.JsonLdCurrencyPath,
                MaxPathLength,
                nameof(configuration.JsonLdCurrencyPath));
        }
        else
        {
            cssSelector = NormalizeRequired(
                configuration.CssSelector,
                MaxSelectorLength,
                nameof(configuration.CssSelector));
            cssValueSource = configuration.CssValueSource
                ?? throw new ArgumentException("CSS value source is required.");
            if (!Enum.IsDefined(cssValueSource.Value))
            {
                throw new ArgumentOutOfRangeException(nameof(configuration.CssValueSource));
            }

            cssAttributeName = cssValueSource == RetailerExtractionCssValueSource.Attribute
                ? NormalizeRequired(
                    configuration.CssAttributeName,
                    MaxAttributeNameLength,
                    nameof(configuration.CssAttributeName))
                : null;
        }

        Name = normalizedName;
        IsEnabled = configuration.IsEnabled;
        RuleType = configuration.RuleType;
        JsonLdObjectType = jsonLdObjectType;
        JsonLdPricePath = jsonLdPricePath;
        JsonLdCurrencyPath = jsonLdCurrencyPath;
        CssSelector = cssSelector;
        CssValueSource = cssValueSource;
        CssAttributeName = cssAttributeName;
        DecimalSeparator = configuration.DecimalSeparator;
        ThousandsSeparator = configuration.ThousandsSeparator;
        ExpectedCurrencyCode = EuroCurrencyCode;
        PriceBasis = configuration.PriceBasis;
        MinimumValue = configuration.MinimumValue;
        MaximumValue = configuration.MaximumValue;
    }

    private static void ValidateRange(decimal? minimum, decimal? maximum)
    {
        foreach (var value in new[] { minimum, maximum }.Where(value => value.HasValue))
        {
            if (value <= 0m
                || value > MaximumAcceptedValue
                || decimal.Round(value.Value, 2) != value.Value)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimum),
                    "Acceptable values must be positive and have at most two decimal places.");
            }
        }

        if (minimum.HasValue && maximum.HasValue && minimum.Value > maximum.Value)
        {
            throw new ArgumentException("Minimum value cannot exceed maximum value.");
        }
    }

    private static string NormalizeRequired(string? value, int maximumLength, string name) =>
        NormalizeOptional(value, maximumLength, name)
        ?? throw new ArgumentException($"{name} is required.", name);

    private static string? NormalizeOptional(string? value, int maximumLength, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maximumLength || normalized.Any(char.IsControl))
        {
            throw new ArgumentException(
                $"{name} must be at most {maximumLength} characters and contain no control characters.",
                name);
        }

        return normalized;
    }
}
