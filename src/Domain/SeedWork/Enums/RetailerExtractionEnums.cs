namespace Domain.SeedWork.Enums;

public enum RetailerExtractionProfileStatus
{
    Draft = 0,
    Published = 1
}

public enum RetailerExtractionRuleType
{
    JsonLd = 0,
    CssSelector = 1
}

public enum RetailerExtractionCssValueSource
{
    TextContent = 0,
    Attribute = 1
}

public enum RetailerExtractionDecimalSeparator
{
    Dot = 0,
    Comma = 1
}

public enum RetailerExtractionThousandsSeparator
{
    None = 0,
    Dot = 1,
    Comma = 2,
    Space = 3
}

public static class RetailerExtractionCodes
{
    public static string ToCode(this RetailerExtractionProfileStatus status) => status switch
    {
        RetailerExtractionProfileStatus.Draft => "draft",
        RetailerExtractionProfileStatus.Published => "published",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    public static string ToCode(this RetailerExtractionRuleType type) => type switch
    {
        RetailerExtractionRuleType.JsonLd => "jsonLd",
        RetailerExtractionRuleType.CssSelector => "cssSelector",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    public static bool TryParseRuleType(string? value, out RetailerExtractionRuleType type)
    {
        type = value?.Trim() switch
        {
            "jsonLd" => RetailerExtractionRuleType.JsonLd,
            "cssSelector" => RetailerExtractionRuleType.CssSelector,
            _ => default
        };
        return value?.Trim() is "jsonLd" or "cssSelector";
    }

    public static string ToCode(this RetailerExtractionCssValueSource source) => source switch
    {
        RetailerExtractionCssValueSource.TextContent => "textContent",
        RetailerExtractionCssValueSource.Attribute => "attribute",
        _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
    };

    public static bool TryParseCssValueSource(
        string? value,
        out RetailerExtractionCssValueSource source)
    {
        source = value?.Trim() switch
        {
            "textContent" => RetailerExtractionCssValueSource.TextContent,
            "attribute" => RetailerExtractionCssValueSource.Attribute,
            _ => default
        };
        return value?.Trim() is "textContent" or "attribute";
    }

    public static string ToCode(this RetailerExtractionDecimalSeparator separator) =>
        separator switch
        {
            RetailerExtractionDecimalSeparator.Dot => "dot",
            RetailerExtractionDecimalSeparator.Comma => "comma",
            _ => throw new ArgumentOutOfRangeException(nameof(separator), separator, null)
        };

    public static bool TryParseDecimalSeparator(
        string? value,
        out RetailerExtractionDecimalSeparator separator)
    {
        separator = value?.Trim() switch
        {
            "dot" => RetailerExtractionDecimalSeparator.Dot,
            "comma" => RetailerExtractionDecimalSeparator.Comma,
            _ => default
        };
        return value?.Trim() is "dot" or "comma";
    }

    public static string ToCode(this RetailerExtractionThousandsSeparator separator) =>
        separator switch
        {
            RetailerExtractionThousandsSeparator.None => "none",
            RetailerExtractionThousandsSeparator.Dot => "dot",
            RetailerExtractionThousandsSeparator.Comma => "comma",
            RetailerExtractionThousandsSeparator.Space => "space",
            _ => throw new ArgumentOutOfRangeException(nameof(separator), separator, null)
        };

    public static bool TryParseThousandsSeparator(
        string? value,
        out RetailerExtractionThousandsSeparator separator)
    {
        separator = value?.Trim() switch
        {
            "none" => RetailerExtractionThousandsSeparator.None,
            "dot" => RetailerExtractionThousandsSeparator.Dot,
            "comma" => RetailerExtractionThousandsSeparator.Comma,
            "space" => RetailerExtractionThousandsSeparator.Space,
            _ => default
        };
        return value?.Trim() is "none" or "dot" or "comma" or "space";
    }
}
