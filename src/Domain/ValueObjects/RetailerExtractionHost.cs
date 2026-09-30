using System.Globalization;
using System.Net;

namespace Domain.ValueObjects;

public sealed record RetailerExtractionHost
{
    public const int MaxLength = 253;

    private RetailerExtractionHost(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static RetailerExtractionHost Create(string value)
    {
        if (!TryCreate(value, out var host))
        {
            throw new ArgumentException(
                "Allowed hosts must be exact DNS hostnames without a scheme, port, path, wildcard, or IP address.",
                nameof(value));
        }

        return host;
    }

    public static bool TryCreate(string? value, out RetailerExtractionHost host)
    {
        host = null!;
        var candidate = value?.Trim().TrimEnd('.');
        if (string.IsNullOrWhiteSpace(candidate)
            || candidate.Length > MaxLength
            || candidate.IndexOfAny(['/', '\\', ':', '*', '@', '?', '#']) >= 0
            || candidate.Any(char.IsWhiteSpace))
        {
            return false;
        }

        try
        {
            var normalized = new IdnMapping().GetAscii(candidate).ToLowerInvariant();
            var labels = normalized.Split('.');
            if (normalized.Length is 0 or > MaxLength
                || IPAddress.TryParse(normalized, out _)
                || Uri.CheckHostName(normalized) != UriHostNameType.Dns
                || labels.Any(label => label.Length is 0 or > 63
                    || label[0] == '-'
                    || label[^1] == '-'
                    || label.Any(character => !char.IsAsciiLetterOrDigit(character)
                        && character != '-')))
            {
                return false;
            }

            host = new RetailerExtractionHost(normalized);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
