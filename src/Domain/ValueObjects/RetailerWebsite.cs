namespace Domain.ValueObjects;

public sealed record RetailerWebsite(string BaseUrl, string NormalizedHost)
{
    public const int MaxBaseUrlLength = 2048;
    public const int MaxHostLength = 253;

    public static RetailerWebsite Create(string value)
    {
        if (!TryCreate(value, out var website))
        {
            throw new ArgumentException(
                "The retailer website must be an absolute HTTP or HTTPS origin without credentials, a path, query, or fragment.",
                nameof(value));
        }

        return website;
    }

    public static bool TryCreate(string? value, out RetailerWebsite website)
    {
        website = null!;
        var candidate = value?.Trim();
        if (string.IsNullOrWhiteSpace(candidate)
            || candidate.Length > MaxBaseUrlLength
            || !Uri.TryCreate(candidate, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            || string.IsNullOrWhiteSpace(uri.Host)
            || !string.IsNullOrEmpty(uri.UserInfo)
            || candidate.Contains('?')
            || candidate.Contains('#')
            || (uri.AbsolutePath.Length > 0 && uri.AbsolutePath != "/")
            || !string.IsNullOrEmpty(uri.Query)
            || !string.IsNullOrEmpty(uri.Fragment))
        {
            return false;
        }

        try
        {
            var host = uri.IdnHost.TrimEnd('.').ToLowerInvariant();
            if (host.Length == 0 || host.Length > MaxHostLength)
            {
                return false;
            }

            var normalizedHost = host.StartsWith("www.", StringComparison.OrdinalIgnoreCase)
                && host.Length > 4
                    ? host[4..]
                    : host;
            var canonicalHost = uri.HostNameType == UriHostNameType.IPv6
                ? $"[{host}]"
                : host;
            var port = uri.IsDefaultPort ? string.Empty : $":{uri.Port}";
            var baseUrl = $"{uri.Scheme.ToLowerInvariant()}://{canonicalHost}{port}";

            if (baseUrl.Length > MaxBaseUrlLength)
            {
                return false;
            }

            website = new RetailerWebsite(baseUrl, normalizedHost);
            return true;
        }
        catch (UriFormatException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
