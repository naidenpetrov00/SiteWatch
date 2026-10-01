using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Application.RetailerExtractionProfiles;
using Application.SeedWork.Interfaces;
using Domain.Entities;
using Domain.SeedWork.Enums;

namespace Infrastructure.RetailerExtractionProfiles.Services;

public sealed partial class RetailerExtractionTestRunner : IRetailerExtractionTestRunner
{
    private const int MaximumRedirects = 5;
    private const int MaximumResponseBytes = 5 * 1024 * 1024;
    private static readonly TimeSpan OverallTimeout = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(5);
    private static readonly IReadOnlySet<string> KnownCurrencyCodes = BuildCurrencyCodes();
    private static readonly string[] CurrencyAttributes =
        ["currency", "data-currency", "data-price-currency", "pricecurrency", "content"];

    public async Task<RetailerExtractionRunnerResult> RunAsync(
        string productUrl,
        IReadOnlySet<string> allowedHosts,
        IReadOnlyList<RetailerExtractionRule> rules,
        CancellationToken cancellationToken)
    {
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(OverallTimeout);
        try
        {
            var initialUri = ValidateUrl(productUrl, allowedHosts);
            var fetched = await FetchHtmlAsync(initialUri, allowedHosts, timeoutSource.Token);
            timeoutSource.Token.ThrowIfCancellationRequested();

            var parser = new HtmlParser();
            IDocument document;
            try
            {
                document = await parser.ParseDocumentAsync(fetched.Html, timeoutSource.Token);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception) when (exception is ArgumentException
                or InvalidOperationException)
            {
                throw new RetailerExtractionTestException(
                    RetailerExtractionTestFailureKind.Content,
                    "The bounded HTML document could not be parsed.",
                    exception);
            }
            timeoutSource.Token.ThrowIfCancellationRequested();

            var diagnostics = new List<RetailerExtractionRuleDiagnosticDto>();
            foreach (var rule in rules.OrderBy(rule => rule.Priority))
            {
                timeoutSource.Token.ThrowIfCancellationRequested();
                var attempt = rule.RuleType == RetailerExtractionRuleType.JsonLd
                    ? EvaluateJsonLd(document, rule)
                    : EvaluateCss(document, rule);
                timeoutSource.Token.ThrowIfCancellationRequested();
                diagnostics.Add(new RetailerExtractionRuleDiagnosticDto(
                    rule.Id,
                    rule.Name,
                    rule.RuleType.ToCode(),
                    rule.Priority,
                    attempt.Outcome,
                    attempt.Explanation));
                if (attempt.Amount.HasValue)
                {
                    return new RetailerExtractionRunnerResult(
                        DateTimeOffset.UtcNow,
                        fetched.FinalUri.AbsoluteUri,
                        new RetailerExtractionRunnerMatch(
                            rule.Id,
                            attempt.Amount.Value,
                            NormalizeRaw(attempt.RawValue!)),
                        diagnostics);
                }
            }

            return new RetailerExtractionRunnerResult(
                DateTimeOffset.UtcNow,
                fetched.FinalUri.AbsoluteUri,
                null,
                diagnostics);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new RetailerExtractionTestException(
                RetailerExtractionTestFailureKind.Timeout,
                "The extraction test exceeded the fifteen-second time limit.",
                exception);
        }
        catch (RetailerExtractionTestException)
        {
            throw;
        }
        catch (IOException exception)
        {
            throw new RetailerExtractionTestException(
                RetailerExtractionTestFailureKind.Network,
                "The remote HTML document could not be downloaded completely.",
                exception);
        }
    }

    private static async Task<FetchedHtml> FetchHtmlAsync(
        Uri initialUri,
        IReadOnlySet<string> allowedHosts,
        CancellationToken cancellationToken)
    {
        var current = initialUri;
        var visited = new HashSet<string>(StringComparer.Ordinal) { current.AbsoluteUri };
        for (var redirects = 0; ; redirects++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var addresses = await ResolveAndValidateAsync(current.IdnHost, cancellationToken);
            using var handler = CreatePinnedHandler(addresses);
            using var client = new HttpClient(handler)
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
            using var request = new HttpRequestMessage(HttpMethod.Get, current);
            request.Version = HttpVersion.Version11;
            request.VersionPolicy = HttpVersionPolicy.RequestVersionExact;
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
            request.Headers.UserAgent.ParseAdd("SiteWatch-Extraction-Test/1.0");

            HttpResponseMessage response;
            try
            {
                response = await client.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (HttpRequestException exception)
            {
                throw new RetailerExtractionTestException(
                    RetailerExtractionTestFailureKind.Network,
                    "The remote server could not be reached securely.",
                    exception);
            }

            using (response)
            {
                if (IsRedirect(response.StatusCode))
                {
                    if (redirects >= MaximumRedirects)
                    {
                        throw new RetailerExtractionTestException(
                            RetailerExtractionTestFailureKind.Network,
                            "The remote server exceeded the redirect limit.");
                    }
                    if (response.Headers.Location is null)
                    {
                        throw new RetailerExtractionTestException(
                            RetailerExtractionTestFailureKind.Network,
                            "The remote server returned an invalid redirect.");
                    }

                    var destination = response.Headers.Location.IsAbsoluteUri
                        ? response.Headers.Location
                        : new Uri(current, response.Headers.Location);
                    current = ValidateUrl(destination.AbsoluteUri, allowedHosts);
                    if (!visited.Add(current.AbsoluteUri))
                    {
                        throw new RetailerExtractionTestException(
                            RetailerExtractionTestFailureKind.Network,
                            "The remote server returned a redirect loop.");
                    }
                    continue;
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw new RetailerExtractionTestException(
                        RetailerExtractionTestFailureKind.Network,
                        "The remote server returned an unsuccessful status.");
                }

                var mediaType = response.Content.Headers.ContentType?.MediaType;
                if (!string.Equals(mediaType, "text/html", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(
                        mediaType,
                        "application/xhtml+xml",
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new RetailerExtractionTestException(
                        RetailerExtractionTestFailureKind.Content,
                        "The remote response is not an HTML document.");
                }
                if (response.Content.Headers.ContentLength is > MaximumResponseBytes)
                {
                    throw new RetailerExtractionTestException(
                        RetailerExtractionTestFailureKind.Content,
                        "The remote HTML document exceeds the five MiB limit.");
                }
                if (response.Content.Headers.ContentEncoding.Count > 0)
                {
                    throw new RetailerExtractionTestException(
                        RetailerExtractionTestFailureKind.Content,
                        "The remote HTML document uses an unsupported content encoding.");
                }

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var buffer = new MemoryStream();
                var chunk = new byte[81920];
                while (true)
                {
                    var read = await stream.ReadAsync(chunk, cancellationToken);
                    if (read == 0) break;
                    if (buffer.Length + read > MaximumResponseBytes)
                    {
                        throw new RetailerExtractionTestException(
                            RetailerExtractionTestFailureKind.Content,
                            "The remote HTML document exceeds the five MiB limit.");
                    }
                    await buffer.WriteAsync(chunk.AsMemory(0, read), cancellationToken);
                }
                if (buffer.Length == 0)
                {
                    throw new RetailerExtractionTestException(
                        RetailerExtractionTestFailureKind.Content,
                        "The remote HTML document is empty.");
                }

                buffer.Position = 0;
                using var reader = new StreamReader(
                    buffer,
                    GetResponseEncoding(response.Content.Headers.ContentType?.CharSet),
                    detectEncodingFromByteOrderMarks: true);
                return new FetchedHtml(
                    current,
                    await reader.ReadToEndAsync(cancellationToken));
            }
        }
    }

    private static SocketsHttpHandler CreatePinnedHandler(IReadOnlyList<IPAddress> addresses) =>
        new()
        {
            AllowAutoRedirect = false,
            UseProxy = false,
            UseCookies = false,
            Credentials = null,
            PreAuthenticate = false,
            ConnectTimeout = ConnectTimeout,
            AutomaticDecompression = DecompressionMethods.GZip
                | DecompressionMethods.Deflate
                | DecompressionMethods.Brotli,
            ConnectCallback = async (context, cancellationToken) =>
            {
                Exception? lastFailure = null;
                foreach (var address in addresses)
                {
                    var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    try
                    {
                        await socket.ConnectAsync(
                            new IPEndPoint(address, context.DnsEndPoint.Port),
                            cancellationToken);
                        return new NetworkStream(socket, ownsSocket: true);
                    }
                    catch (Exception exception) when (exception is SocketException
                        or OperationCanceledException)
                    {
                        socket.Dispose();
                        lastFailure = exception;
                        if (exception is OperationCanceledException) throw;
                    }
                }

                throw new HttpRequestException(
                    "No validated destination accepted the connection.",
                    lastFailure);
            }
        };

    private static Uri ValidateUrl(string value, IReadOnlySet<string> allowedHosts)
    {
        if (string.IsNullOrWhiteSpace(value)
            || value.Length > RetailerListing.MaxProductUrlLength
            || value.Any(char.IsControl)
            || !Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttps
            || string.IsNullOrWhiteSpace(uri.Host)
            || uri.HostNameType != UriHostNameType.Dns
            || !string.IsNullOrEmpty(uri.UserInfo)
            || !string.IsNullOrEmpty(uri.Fragment)
            || (!uri.IsDefaultPort && uri.Port != 443))
        {
            throw new RetailerExtractionTestException(
                RetailerExtractionTestFailureKind.Security,
                "Only absolute HTTPS URLs without credentials, fragments, or non-standard ports are allowed.");
        }

        string normalizedHost;
        try
        {
            normalizedHost = new IdnMapping().GetAscii(uri.IdnHost)
                .TrimEnd('.')
                .ToLowerInvariant();
        }
        catch (ArgumentException exception)
        {
            throw new RetailerExtractionTestException(
                RetailerExtractionTestFailureKind.Security,
                "The URL hostname is malformed.",
                exception);
        }

        if (IPAddress.TryParse(normalizedHost, out _)
            || !normalizedHost.Contains('.')
            || IsInternalHostname(normalizedHost)
            || !allowedHosts.Contains(normalizedHost))
        {
            throw new RetailerExtractionTestException(
                RetailerExtractionTestFailureKind.Security,
                "The URL hostname is not permitted by the saved draft configuration.");
        }

        var builder = new UriBuilder(uri) { Host = normalizedHost };
        return builder.Uri;
    }

    private static async Task<IReadOnlyList<IPAddress>> ResolveAndValidateAsync(
        string host,
        CancellationToken cancellationToken)
    {
        IPAddress[] addresses;
        try
        {
            addresses = await Dns.GetHostAddressesAsync(host, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (exception is SocketException or ArgumentException)
        {
            throw new RetailerExtractionTestException(
                RetailerExtractionTestFailureKind.Network,
                "The permitted hostname could not be resolved.",
                exception);
        }

        if (addresses.Length == 0)
        {
            throw new RetailerExtractionTestException(
                RetailerExtractionTestFailureKind.Network,
                "The permitted hostname did not resolve to a destination.");
        }
        if (addresses.Any(address => !IsGlobalUnicast(address)))
        {
            throw new RetailerExtractionTestException(
                RetailerExtractionTestFailureKind.Security,
                "The hostname resolves to a non-public destination.");
        }

        return addresses.Distinct().ToArray();
    }

    private static bool IsInternalHostname(string host) =>
        host == "localhost"
        || host.EndsWith(".localhost", StringComparison.Ordinal)
        || host.EndsWith(".local", StringComparison.Ordinal)
        || host.EndsWith(".internal", StringComparison.Ordinal)
        || host.EndsWith(".home", StringComparison.Ordinal)
        || host.EndsWith(".lan", StringComparison.Ordinal)
        || host.EndsWith(".corp", StringComparison.Ordinal)
        || host.EndsWith(".intranet", StringComparison.Ordinal)
        || host.EndsWith(".localdomain", StringComparison.Ordinal)
        || host.EndsWith(".test", StringComparison.Ordinal)
        || host.EndsWith(".invalid", StringComparison.Ordinal)
        || host.EndsWith(".example", StringComparison.Ordinal)
        || host.EndsWith(".alt", StringComparison.Ordinal)
        || host.EndsWith(".onion", StringComparison.Ordinal)
        || host.EndsWith(".arpa", StringComparison.Ordinal);

    private static bool IsGlobalUnicast(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6) return IsGlobalUnicast(address.MapToIPv4());
        var bytes = address.GetAddressBytes();
        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            return bytes[0] is not 0 and not 10 and not 127
                && !(bytes[0] == 100 && bytes[1] is >= 64 and <= 127)
                && !(bytes[0] == 169 && bytes[1] == 254)
                && !(bytes[0] == 172 && bytes[1] is >= 16 and <= 31)
                && !(bytes[0] == 192 && bytes[1] == 168)
                && !(bytes[0] == 192 && bytes[1] == 0)
                && !(bytes[0] == 192 && bytes[1] == 31 && bytes[2] == 196)
                && !(bytes[0] == 192 && bytes[1] == 52 && bytes[2] == 193)
                && !(bytes[0] == 192 && bytes[1] == 88 && bytes[2] == 99)
                && !(bytes[0] == 192 && bytes[1] == 175 && bytes[2] == 48)
                && !(bytes[0] == 198 && bytes[1] is 18 or 19)
                && !(bytes[0] == 198 && bytes[1] == 51 && bytes[2] == 100)
                && !(bytes[0] == 203 && bytes[1] == 0 && bytes[2] == 113)
                && bytes[0] < 224;
        }

        if (address.AddressFamily != AddressFamily.InterNetworkV6
            || address.Equals(IPAddress.IPv6None)
            || address.Equals(IPAddress.IPv6Loopback)
            || address.IsIPv6LinkLocal
            || address.IsIPv6Multicast
            || address.IsIPv6SiteLocal)
        {
            return false;
        }

        var isGlobalRange = (bytes[0] & 0xE0) == 0x20;
        var isUniqueLocal = (bytes[0] & 0xFE) == 0xFC;
        var isDocumentation = bytes[0] == 0x20 && bytes[1] == 0x01
            && bytes[2] == 0x0D && bytes[3] == 0xB8;
        var isProtocolAssignment = bytes[0] == 0x20 && bytes[1] == 0x01
            && bytes[2] <= 0x01;
        var isSixToFour = bytes[0] == 0x20 && bytes[1] == 0x02;
        var isDocumentationTwo = bytes[0] == 0x3F && bytes[1] == 0xFF
            && (bytes[2] & 0xF0) == 0;
        return isGlobalRange && !isUniqueLocal && !isDocumentation
            && !isProtocolAssignment && !isSixToFour && !isDocumentationTwo;
    }

    private static RuleAttempt EvaluateJsonLd(IDocument document, RetailerExtractionRule rule)
    {
        if (string.IsNullOrWhiteSpace(rule.JsonLdObjectType)
            || !TryParsePath(rule.JsonLdPricePath, out var pricePath)
            || (rule.JsonLdCurrencyPath is not null
                && !TryParsePath(rule.JsonLdCurrencyPath, out _)))
        {
            return RuleAttempt.Fail("invalidConfiguration", "The configured JSON-LD path is invalid.");
        }

        var sawScript = false;
        var sawValidJson = false;
        var sawMatchingObject = false;
        RuleAttempt? firstFailure = null;
        foreach (var script in document.QuerySelectorAll("script").Where(script =>
            string.Equals(
                script.GetAttribute("type")?.Split(';', 2)[0].Trim(),
                "application/ld+json",
                StringComparison.OrdinalIgnoreCase)))
        {
            sawScript = true;
            try
            {
                using var json = JsonDocument.Parse(script.TextContent);
                sawValidJson = true;
                foreach (var candidateObject in EnumerateObjects(json.RootElement))
                {
                    if (!HasType(candidateObject, rule.JsonLdObjectType!)) continue;
                    sawMatchingObject = true;
                    foreach (var candidate in ResolvePath(candidateObject, pricePath))
                    {
                        var raw = candidate.Value.ValueKind switch
                        {
                            JsonValueKind.String => candidate.Value.GetString(),
                            JsonValueKind.Number => candidate.Value.GetRawText(),
                            _ => null
                        };
                        if (string.IsNullOrWhiteSpace(raw))
                        {
                            firstFailure ??= RuleAttempt.Fail(
                                "missingValue", "The configured JSON-LD price value is empty.");
                            continue;
                        }

                        var currency = ResolveJsonCurrency(candidate, rule.JsonLdCurrencyPath);
                        var evaluated = EvaluateAmount(raw, currency, rule, candidate.Value.ValueKind == JsonValueKind.Number);
                        if (evaluated.Amount.HasValue) return evaluated;
                        firstFailure ??= evaluated;
                    }
                }
            }
            catch (JsonException)
            {
                // Invalid blocks are isolated from other JSON-LD blocks.
            }
        }

        if (!sawScript) return RuleAttempt.Fail("noMatch", "No JSON-LD block was found.");
        if (!sawValidJson) return RuleAttempt.Fail("invalidJsonLd", "The JSON-LD blocks are not valid JSON.");
        if (!sawMatchingObject) return RuleAttempt.Fail("noMatch", "No matching JSON-LD object was found.");
        return firstFailure
            ?? RuleAttempt.Fail("missingValue", "The configured JSON-LD price path produced no value.");
    }

    private static RuleAttempt EvaluateCss(IDocument document, RetailerExtractionRule rule)
    {
        if (string.IsNullOrWhiteSpace(rule.CssSelector)
            || !rule.CssValueSource.HasValue
            || !Enum.IsDefined(rule.CssValueSource.Value)
            || (rule.CssValueSource == RetailerExtractionCssValueSource.Attribute
                && string.IsNullOrWhiteSpace(rule.CssAttributeName)))
        {
            return RuleAttempt.Fail(
                "invalidConfiguration", "The configured CSS extraction source is invalid.");
        }

        IHtmlCollection<IElement> elements;
        try
        {
            elements = document.QuerySelectorAll(rule.CssSelector!);
        }
        catch (Exception exception) when (exception is AngleSharp.Dom.DomException or ArgumentException)
        {
            return RuleAttempt.Fail("invalidConfiguration", "The configured CSS selector is invalid.");
        }

        foreach (var element in elements)
        {
            var raw = rule.CssValueSource == RetailerExtractionCssValueSource.Attribute
                ? element.GetAttribute(rule.CssAttributeName!)
                : element.TextContent;
            if (string.IsNullOrWhiteSpace(raw)) continue;
            return EvaluateAmount(raw, ResolveCssCurrency(element, raw), rule, invariantNumber: false);
        }

        return elements.Length == 0
            ? RuleAttempt.Fail("noMatch", "The CSS selector matched no element.")
            : rule.CssValueSource == RetailerExtractionCssValueSource.Attribute
                ? RuleAttempt.Fail(
                    "missingAttribute", "The matched elements have no non-empty configured attribute.")
                : RuleAttempt.Fail(
                    "missingValue", "The matched elements have no non-empty text content.");
    }

    private static RuleAttempt EvaluateAmount(
        string raw,
        string? currency,
        RetailerExtractionRule rule,
        bool invariantNumber)
    {
        currency ??= DetectCurrency(raw, allowNumericOnly: false);
        if (currency is not null && !currency.Equals("EUR", StringComparison.OrdinalIgnoreCase))
        {
            return RuleAttempt.Fail(
                "currencyMismatch", "The selected price is explicitly associated with a non-EUR currency.");
        }
        if (!TryParseAmount(raw, rule, invariantNumber, out var amount))
        {
            return RuleAttempt.Fail(
                "amountParsingFailed", "The selected value does not match the configured number separators.");
        }
        if (amount <= 0m
            || amount > RetailerExtractionRule.MaximumAcceptedValue
            || decimal.Round(amount, 2) != amount
            || (rule.MinimumValue.HasValue && amount < rule.MinimumValue.Value)
            || (rule.MaximumValue.HasValue && amount > rule.MaximumValue.Value))
        {
            return RuleAttempt.Fail(
                "rangeFailed", "The parsed amount is outside the configured positive EUR range.");
        }

        return new RuleAttempt("success", "The rule extracted a valid EUR amount.", amount, raw);
    }

    private static bool TryParseAmount(
        string raw,
        RetailerExtractionRule rule,
        bool invariantNumber,
        out decimal amount)
    {
        amount = 0m;
        if (invariantNumber)
        {
            return decimal.TryParse(
                raw,
                NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent,
                CultureInfo.InvariantCulture,
                out amount);
        }

        var decimalCharacter = rule.DecimalSeparator == RetailerExtractionDecimalSeparator.Dot
            ? "."
            : ",";
        var thousandsPattern = rule.ThousandsSeparator switch
        {
            RetailerExtractionThousandsSeparator.Dot => "\\.",
            RetailerExtractionThousandsSeparator.Comma => ",",
            RetailerExtractionThousandsSeparator.Space => "[ \\u00a0\\u202f]",
            _ => null
        };
        var integerPattern = thousandsPattern is null
            ? "\\d+"
            : $"(?:\\d{{1,3}}(?:{thousandsPattern}\\d{{3}})+|\\d+)";
        var configuredPattern = new Regex(
            $"^{integerPattern}(?:{Regex.Escape(decimalCharacter)}\\d{{1,2}})?$",
            RegexOptions.CultureInvariant);
        var candidate = NumericCandidateRegex().Matches(raw)
            .Cast<Match>()
            .Select(match => match.Value.Trim())
            .FirstOrDefault(value => configuredPattern.IsMatch(value));
        if (candidate is null) return false;

        var normalized = candidate;
        if (thousandsPattern is not null)
        {
            normalized = rule.ThousandsSeparator == RetailerExtractionThousandsSeparator.Space
                ? WhitespaceRegex().Replace(normalized, string.Empty)
                : normalized.Replace(
                    rule.ThousandsSeparator == RetailerExtractionThousandsSeparator.Dot ? "." : ",",
                    string.Empty,
                    StringComparison.Ordinal);
        }
        if (decimalCharacter == ",") normalized = normalized.Replace(',', '.');
        return decimal.TryParse(
            normalized,
            NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out amount);
    }

    private static string? ResolveJsonCurrency(PathCandidate candidate, string? configuredPath)
    {
        if (configuredPath is not null && TryParsePath(configuredPath, out var path))
        {
            var resolved = ResolvePath(candidate.Container, path)
                .Select(item => ScalarString(item.Value))
                .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
            if (resolved is not null) return resolved;

            // A full path such as "offers.priceCurrency" is paired to the selected
            // offer by resolving only its terminal property on that price container.
            return candidate.Container.ValueKind == JsonValueKind.Object
                && candidate.Container.TryGetProperty(path[^1], out var branchCurrency)
                    ? ScalarString(branchCurrency)
                    : null;
        }
        return candidate.Container.ValueKind == JsonValueKind.Object
            && candidate.Container.TryGetProperty("priceCurrency", out var currency)
                ? ScalarString(currency)
                : null;
    }

    private static string? ResolveCssCurrency(IElement element, string raw)
    {
        foreach (var attribute in CurrencyAttributes)
        {
            var value = element.GetAttribute(attribute);
            var currency = DetectCurrency(value, attribute == "content");
            if (currency is not null) return currency;
            if (attribute != "content" && !string.IsNullOrWhiteSpace(value))
            {
                return "NON_EUR";
            }
        }
        return DetectCurrency(raw, allowNumericOnly: false);
    }

    private static string? DetectCurrency(string? value, bool allowNumericOnly)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var upper = value.ToUpperInvariant();
        if (upper.Contains("EUR", StringComparison.Ordinal) || upper.Contains('€')) return "EUR";
        foreach (Match match in CurrencyCodeRegex().Matches(upper))
        {
            if (KnownCurrencyCodes.Contains(match.Value)) return match.Value;
        }
        if (upper.Contains('$')) return "USD";
        if (upper.Contains('£')) return "GBP";
        if (upper.Contains('¥')) return "JPY";
        if (upper.Contains("ЛВ", StringComparison.Ordinal)
            || upper.Contains("ЛЕВ", StringComparison.Ordinal)
            || upper.Contains("KČ", StringComparison.Ordinal)
            || upper.Contains("ZŁ", StringComparison.Ordinal))
        {
            return "NON_EUR";
        }
        if (upper.Any(character =>
            CharUnicodeInfo.GetUnicodeCategory(character)
                == UnicodeCategory.CurrencySymbol))
        {
            return "NON_EUR";
        }
        if (!allowNumericOnly && Regex.IsMatch(upper.Trim(), "^[A-Z]{3}$")) return upper.Trim();
        return null;
    }

    private static IEnumerable<JsonElement> EnumerateObjects(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            yield return element;
            foreach (var property in element.EnumerateObject())
            {
                foreach (var nested in EnumerateObjects(property.Value)) yield return nested;
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                foreach (var nested in EnumerateObjects(item)) yield return nested;
            }
        }
    }

    private static bool HasType(JsonElement element, string configuredType)
    {
        if (!element.TryGetProperty("@type", out var type)) return false;
        var expected = NormalizeSchemaType(configuredType);
        return type.ValueKind switch
        {
            JsonValueKind.String => NormalizeSchemaType(type.GetString()!) == expected,
            JsonValueKind.Array => type.EnumerateArray().Any(item =>
                item.ValueKind == JsonValueKind.String
                && NormalizeSchemaType(item.GetString()!) == expected),
            _ => false
        };
    }

    private static string NormalizeSchemaType(string value)
    {
        var trimmed = value.Trim().TrimEnd('/');
        var index = Math.Max(
            Math.Max(trimmed.LastIndexOf('/'), trimmed.LastIndexOf(':')),
            trimmed.LastIndexOf('#'));
        return (index >= 0 ? trimmed[(index + 1)..] : trimmed).ToLowerInvariant();
    }

    private static bool TryParsePath(string? value, out string[] segments)
    {
        segments = value?.Split('.', StringSplitOptions.TrimEntries) ?? [];
        return segments.Length > 0 && segments.All(segment => PathSegmentRegex().IsMatch(segment));
    }

    private static IEnumerable<PathCandidate> ResolvePath(JsonElement root, IReadOnlyList<string> segments)
    {
        return Resolve(root, 0, root);

        IEnumerable<PathCandidate> Resolve(JsonElement current, int index, JsonElement container)
        {
            if (current.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in current.EnumerateArray())
                {
                    foreach (var result in Resolve(item, index, container)) yield return result;
                }
                yield break;
            }
            if (index == segments.Count)
            {
                yield return new PathCandidate(current, container);
                yield break;
            }
            if (current.ValueKind != JsonValueKind.Object
                || !current.TryGetProperty(segments[index], out var next))
            {
                yield break;
            }

            var nextContainer = index == segments.Count - 1 ? current : container;
            foreach (var result in Resolve(next, index + 1, nextContainer)) yield return result;
        }
    }

    private static string? ScalarString(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString(),
        JsonValueKind.Number => value.GetRawText(),
        _ => null
    };

    private static bool IsRedirect(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.MovedPermanently
            or HttpStatusCode.Redirect
            or HttpStatusCode.RedirectMethod
            or HttpStatusCode.TemporaryRedirect
            or HttpStatusCode.PermanentRedirect;

    private static string NormalizeRaw(string raw)
    {
        var normalized = WhitespaceRegex().Replace(raw.Trim(), " ");
        return normalized.Length <= 256 ? normalized : normalized[..256];
    }

    [GeneratedRegex("\\s+", RegexOptions.CultureInvariant)]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex("[0-9](?:[0-9.,\\u00a0\\u202f ]*[0-9])?", RegexOptions.CultureInvariant)]
    private static partial Regex NumericCandidateRegex();

    [GeneratedRegex("(?<![A-Z])[A-Z]{3}(?![A-Z])", RegexOptions.CultureInvariant)]
    private static partial Regex CurrencyCodeRegex();

    [GeneratedRegex("^[A-Za-z_@][A-Za-z0-9_@-]*$", RegexOptions.CultureInvariant)]
    private static partial Regex PathSegmentRegex();

    private static System.Text.Encoding GetResponseEncoding(string? charset)
    {
        if (string.IsNullOrWhiteSpace(charset)) return System.Text.Encoding.UTF8;
        try
        {
            return System.Text.Encoding.GetEncoding(charset.Trim(' ', '"', '\''));
        }
        catch (ArgumentException exception)
        {
            throw new RetailerExtractionTestException(
                RetailerExtractionTestFailureKind.Content,
                "The remote HTML document declares an unsupported character encoding.",
                exception);
        }
    }

    private static IReadOnlySet<string> BuildCurrencyCodes()
    {
        var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "EUR" };
        foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
        {
            try
            {
                codes.Add(new RegionInfo(culture.Name).ISOCurrencySymbol);
            }
            catch (ArgumentException)
            {
                // Ignore cultures without a region-backed ISO currency.
            }
        }
        return codes;
    }

    private sealed record FetchedHtml(Uri FinalUri, string Html);
    private sealed record PathCandidate(JsonElement Value, JsonElement Container);
    private sealed record RuleAttempt(
        string Outcome,
        string Explanation,
        decimal? Amount,
        string? RawValue)
    {
        public static RuleAttempt Fail(string outcome, string explanation) =>
            new(outcome, explanation, null, null);
    }
}
