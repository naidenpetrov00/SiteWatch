using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.SeedWork.Exceptions;
using Application.SeedWork.Converters;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.External;
using Ardalis.GuardClauses;
using Infrastructure.SeedWork.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;

namespace Infrastructure.InvoiceExtraction;

public sealed class OpenRouterInvoiceExtractor : IInvoiceExtractor
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter(),
            new FlexibleDateTimeOffsetConverter()
        }
    };

    private readonly HttpClient httpClient;
    private readonly OpenRouterOptions openRouterOptions;
    private readonly ILogger<OpenRouterInvoiceExtractor> logger;

    public OpenRouterInvoiceExtractor(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OpenRouterInvoiceExtractor> logger)
    {
        this.httpClient = httpClient;
        this.logger = logger;
        openRouterOptions = configuration.GetOptions<OpenRouterOptions>();
    }

    public async Task<InvoiceExtractionResult?> ExtractAsync(
        Stream stream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var apiKey = Guard.Against.NullOrEmpty(openRouterOptions.ApiKey, nameof(openRouterOptions.ApiKey));
        var model = Guard.Against.NullOrEmpty(openRouterOptions.Model, nameof(openRouterOptions.Model));
        var parserEngine = Guard.Against.NullOrEmpty(
            openRouterOptions.PdfParserEngine,
            nameof(openRouterOptions.PdfParserEngine));

        var normalizedFile = await NormalizeInputAsync(stream, contentType, cancellationToken);
        logger.LogDebug(
            "OpenRouter extraction starting. Model={Model}, ParserEngine={ParserEngine}, File={FileName}, ContentType={ContentType}, Size={Size}",
            model,
            parserEngine,
            normalizedFile.FileName,
            contentType,
            normalizedFile.Content.Length);
        var pdfDataUrl = $"data:application/pdf;base64,{Convert.ToBase64String(normalizedFile.Content)}";

        var responseContent = await SendExtractionRequestAsync(
            apiKey,
            model,
            parserEngine,
            normalizedFile.FileName,
            pdfDataUrl,
            cancellationToken);

        var parsedText = ExtractAnnotationText(responseContent);
        if (string.IsNullOrWhiteSpace(parsedText))
        {
            throw new OpenRouterInvoiceExtractionException(
                "OpenRouter returned no OCR text annotations for the uploaded document.",
                responseContent);
        }

        logger.LogDebug(
            "OpenRouter OCR text preview: {Preview}",
            parsedText.Length > 800 ? parsedText[..800] : parsedText);

        var extractionResponseContent = await SendExtractionTextRequestAsync(
            apiKey,
            model,
            parsedText,
            cancellationToken);

        return ParseExtractionResult(extractionResponseContent, parsedText);
    }

    private async Task<string> SendExtractionRequestAsync(
        string apiKey,
        string model,
        string parserEngine,
        string fileName,
        string pdfDataUrl,
        CancellationToken cancellationToken)
    {
        try
        {
            return await SendExtractionRequestAsync(
                apiKey,
                model,
                parserEngine,
                fileName,
                pdfDataUrl,
                BuildResponseFormat("json_schema"),
                cancellationToken);
        }
        catch (OpenRouterInvoiceExtractionException ex) when (TryFallbackToJsonObject(ex))
        {
            return await SendExtractionRequestAsync(
                apiKey,
                model,
                parserEngine,
                fileName,
                pdfDataUrl,
                BuildResponseFormat("json_object"),
                cancellationToken);
        }
    }

    private async Task<string> SendExtractionRequestAsync(
        string apiKey,
        string model,
        string parserEngine,
        string fileName,
        string pdfDataUrl,
        object? responseFormat,
        CancellationToken cancellationToken)
    {
        var requestBody = new
        {
            model,
            temperature = 0,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = """
                    You extract invoice-like documents into strict JSON.
                    Return only one JSON object.
                    Read the entire parsed document carefully before answering.
                    Use only values that are explicitly visible in the document text.
                    Do not guess, infer, normalize, or fabricate any value.
                    If a field is not clearly present in the OCR/text, return null.
                    Never fill fields with generic example values, template values, or typical invoice defaults.
                    If the document content is insufficient, return Unknown with null values.
                    The document may be in Bulgarian. Interpret Bulgarian labels correctly.
                    Map these common Bulgarian terms:
                    - Получател means buyer
                    - Доставчик means supplier
                    - ЕИК means company identification number
                    - ИН ДДС or ДДС No means VAT number
                    - МОЛ means contact person or responsible person
                    - Фактура means invoice
                    - Оферта means offer
                    - Касова бележка means receipt
                    - Дата means date
                    Preserve product names and descriptions exactly as written.
                    Extract every visible product or service line, even if only partial.
                    Detect document type as Invoice, Receipt, Offer, or Unknown.
                    Add issues for unclear, suspicious, contradictory, or partially visible fields.
                    Return confidence in every extracted value between 0 and 1.
                    If the document shows both EUR and BGN for the same amount, use the EUR value for monetary fields.
                    If a field is present but uncertain, prefer null and add an issue rather than inventing a value.
                    """
                },
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "text",
                            text = """
                            Extract the document into the response schema using the following rules:
                            - Return all top-level invoice fields.
                            - Return every line item you can identify.
                            - Use null whenever a field is not explicitly visible in the OCR text.
                            - If a value is partially visible, return null and add an issue unless the visible part is unambiguous.
                            - Do not infer supplier, buyer, invoice number, dates, totals, or line items from logos, branding, or domain knowledge.
                            - If the OCR text does not contain line items, return an empty array and add an issue explaining that no items were visible.
                            - If the document uses Bulgarian labels, map them to the matching English field names.
                            - If the OCR text shows both EUR and BGN for the same amount, choose the EUR amount for monetary fields.
                            - Prefer null over wrong values.
                            - Do not use placeholders or invented examples.
                            """
                        },
                        new
                        {
                            type = "file",
                            file = new
                            {
                                filename = fileName,
                                file_data = pdfDataUrl
                            }
                        }
                    }
                }
            },
            plugins = new object[]
            {
                new
                {
                    id = "file-parser",
                    pdf = new
                    {
                        engine = parserEngine
                    }
                }
            },
            response_format = responseFormat,
            stream = false
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        logger.LogDebug(
            "OpenRouter response received. StatusCode={StatusCode}, Length={Length}",
            (int)response.StatusCode,
            responseContent.Length);

        if (!response.IsSuccessStatusCode)
        {
            throw new OpenRouterInvoiceExtractionException(
                $"OpenRouter invoice extraction failed with status code {(int)response.StatusCode}.",
                responseContent,
                (int)response.StatusCode);
        }

        return responseContent;
    }

    private async Task<string> SendExtractionTextRequestAsync(
        string apiKey,
        string model,
        string parsedText,
        CancellationToken cancellationToken)
    {
        var requestBody = new
        {
            model,
            temperature = 0,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = """
                    You extract invoice-like documents into strict JSON.
                    Return only one JSON object.
                    Use only values that are explicitly visible in the OCR text.
                    Do not guess, infer, normalize, or fabricate any value.
                    If a field is not clearly present in the OCR text, return null.
                    The document may be in Bulgarian. Interpret Bulgarian labels correctly.
                    Map these common Bulgarian terms:
                    - Получател means buyer
                    - Доставчик means supplier
                    - ЕИК means company identification number
                    - ИН ДДС or ДДС No means VAT number
                    - МОЛ means contact person or responsible person
                    - Фактура means invoice
                    - Оферта means offer
                    - Касова бележка means receipt
                    - Дата means date
                    Preserve product names and descriptions exactly as written.
                    Return confidence in every extracted value between 0 and 1.
                    If a field is uncertain, prefer null and add an issue rather than inventing a value.
                    """
                },
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "text",
                            text = """
                            Extract the invoice data from the OCR text below.
                            Return the data using the response schema.
                            Only use values that are present in the OCR text.
                            """
                        },
                        new
                        {
                            type = "text",
                            text = parsedText
                        }
                    }
                }
            },
            response_format = BuildResponseFormat("json_schema"),
            stream = false
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        logger.LogDebug(
            "OpenRouter text extraction response received. StatusCode={StatusCode}, Length={Length}",
            (int)response.StatusCode,
            responseContent.Length);

        if (!response.IsSuccessStatusCode)
        {
            throw new OpenRouterInvoiceExtractionException(
                $"OpenRouter invoice extraction failed with status code {(int)response.StatusCode}.",
                responseContent,
                (int)response.StatusCode);
        }

        return responseContent;
    }

    private static bool TryFallbackToJsonObject(OpenRouterInvoiceExtractionException exception)
    {
        if (exception.StatusCode is not (400 or 422))
        {
            return false;
        }

        return exception.RawResponse.Contains("response_format", StringComparison.OrdinalIgnoreCase) ||
               exception.RawResponse.Contains("structured", StringComparison.OrdinalIgnoreCase) ||
               exception.RawResponse.Contains("json_schema", StringComparison.OrdinalIgnoreCase);
    }

    private static object BuildResponseFormat(string type)
        => type switch
        {
            "json_schema" => new
            {
                type = "json_schema",
                json_schema = new
                {
                    name = "invoice_extraction_result",
                    strict = true,
                    schema = new
                    {
                        type = "object",
                        additionalProperties = false,
                        required = new[]
                        {
                            "documentType",
                            "documentTypeConfidence",
                            "supplierName",
                            "supplierNameConfidence",
                            "supplierEik",
                            "supplierEikConfidence",
                            "supplierVatNumber",
                            "supplierVatNumberConfidence",
                            "buyerName",
                            "buyerNameConfidence",
                            "invoiceNumber",
                            "invoiceNumberConfidence",
                            "invoiceDate",
                            "invoiceDateConfidence",
                            "currency",
                            "currencyConfidence",
                            "netTotal",
                            "netTotalConfidence",
                            "vatTotal",
                            "vatTotalConfidence",
                            "grossTotal",
                            "grossTotalConfidence",
                            "overallConfidence",
                            "items",
                            "issues",
                            "rawJson"
                        },
                        properties = new
                        {
                            documentType = new { type = "string", @enum = new[] { "Invoice", "Receipt", "Offer", "Unknown" } },
                            documentTypeConfidence = new { type = "number" },
                            supplierName = new { type = new[] { "string", "null" } },
                            supplierNameConfidence = new { type = new[] { "number", "null" } },
                            supplierEik = new { type = new[] { "string", "null" } },
                            supplierEikConfidence = new { type = new[] { "number", "null" } },
                            supplierVatNumber = new { type = new[] { "string", "null" } },
                            supplierVatNumberConfidence = new { type = new[] { "number", "null" } },
                            buyerName = new { type = new[] { "string", "null" } },
                            buyerNameConfidence = new { type = new[] { "number", "null" } },
                            invoiceNumber = new { type = new[] { "string", "null" } },
                            invoiceNumberConfidence = new { type = new[] { "number", "null" } },
                            invoiceDate = new { type = new[] { "string", "null" } },
                            invoiceDateConfidence = new { type = new[] { "number", "null" } },
                            currency = new { type = new[] { "string", "null" } },
                            currencyConfidence = new { type = new[] { "number", "null" } },
                            netTotal = new { type = new[] { "number", "null" } },
                            netTotalConfidence = new { type = new[] { "number", "null" } },
                            vatTotal = new { type = new[] { "number", "null" } },
                            vatTotalConfidence = new { type = new[] { "number", "null" } },
                            grossTotal = new { type = new[] { "number", "null" } },
                            grossTotalConfidence = new { type = new[] { "number", "null" } },
                            overallConfidence = new { type = new[] { "number", "null" } },
                            items = new
                            {
                                type = "array",
                                items = new
                                {
                                    type = "object",
                                    additionalProperties = false,
                                    required = new[]
                                    {
                                        "productCode",
                                        "productName",
                                        "quantity",
                                        "unit",
                                        "unitPrice",
                                        "discount",
                                        "vatRate",
                                        "lineTotal",
                                        "confidence"
                                    },
                                    properties = new
                                    {
                                        productCode = new { type = new[] { "string", "null" } },
                                        productName = new { type = new[] { "string", "null" } },
                                        quantity = new { type = new[] { "number", "null" } },
                                        unit = new { type = new[] { "string", "null" } },
                                        unitPrice = new { type = new[] { "number", "null" } },
                                        discount = new { type = new[] { "number", "null" } },
                                        vatRate = new { type = new[] { "number", "null" } },
                                        lineTotal = new { type = new[] { "number", "null" } },
                                        confidence = new { type = new[] { "number", "null" } }
                                    }
                                }
                            },
                            issues = new
                            {
                                type = "array",
                                items = new
                                {
                                    type = "object",
                                    additionalProperties = false,
                                    required = new[] { "fieldPath", "extractedValue", "reason", "confidence" },
                                    properties = new
                                    {
                                        fieldPath = new { type = "string" },
                                        extractedValue = new { type = new[] { "string", "null" } },
                                        reason = new { type = "string" },
                                        confidence = new { type = new[] { "number", "null" } }
                                    }
                                }
                            },
                            rawJson = new { type = new[] { "string", "null" } }
                        }
                    }
                }
            },
            "json_object" => new
            {
                type = "json_object"
            },
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

    private InvoiceExtractionResult? ParseExtractionResult(string responseContent, string? ocrText = null)
    {
        using var rootDocument = JsonDocument.Parse(responseContent);
        var root = rootDocument.RootElement;
        var annotations = ExtractFileAnnotations(root);
        if (annotations.Count > 0)
        {
            logger.LogDebug(
                "OpenRouter returned {Count} file annotations. Hashes={Hashes}",
                annotations.Count,
                string.Join(", ", annotations.Select(annotation => annotation.Hash)));
            foreach (var annotation in annotations)
            {
                logger.LogDebug(
                    "OpenRouter annotation {Hash} text preview: {Preview}",
                    annotation.Hash,
                    annotation.Content.Length > 400 ? annotation.Content[..400] : annotation.Content);
            }
        }
        else
        {
            logger.LogDebug("OpenRouter returned no file annotations.");
        }

        if (root.TryGetProperty("error", out var errorElement))
        {
            var providerMessage = errorElement.TryGetProperty("message", out var messageElement) &&
                                  messageElement.ValueKind == JsonValueKind.String
                ? messageElement.GetString()
                : "Provider returned error";
            var providerCode = errorElement.TryGetProperty("code", out var codeElement) &&
                               codeElement.ValueKind == JsonValueKind.Number &&
                               codeElement.TryGetInt32(out var code)
                ? (int?)code
                : null;

            throw new OpenRouterInvoiceExtractionException(
                $"OpenRouter provider error: {providerMessage}",
                responseContent,
                providerCode);
        }

        OpenRouterChatCompletionResponse? completion;
        try
        {
            completion = JsonSerializer.Deserialize<OpenRouterChatCompletionResponse>(
                responseContent,
                JsonSerializerOptions);
        }
        catch (JsonException ex)
        {
            throw new OpenRouterInvoiceExtractionException(
                "OpenRouter returned an invalid response envelope.",
                responseContent,
                innerException: ex);
        }

        var content = NormalizeJsonContent(completion?.Choices.FirstOrDefault()?.Message?.Content);
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new OpenRouterInvoiceExtractionException(
                "OpenRouter returned an empty extraction payload.",
                responseContent);
        }

        try
        {
            var extractionResult = JsonSerializer.Deserialize<InvoiceExtractionResult>(content, JsonSerializerOptions);
            if (extractionResult is null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(ocrText))
            {
                extractionResult = MergeWithOcrFallback(extractionResult, ocrText);
            }

            return extractionResult with { RawJson = content };
        }
        catch (JsonException ex)
        {
            throw new OpenRouterInvoiceExtractionException(
                "OpenRouter returned invalid invoice JSON.",
                responseContent,
                innerException: ex);
        }
    }

    private static async Task<NormalizedInput> NormalizeInputAsync(
        Stream stream,
        string contentType,
        CancellationToken cancellationToken)
    {
        await using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);
        var inputBytes = memoryStream.ToArray();
        var normalizedContentType = NormalizeContentType(contentType);

        if (normalizedContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            await using var inputStream = new MemoryStream(inputBytes);
            using var image = await Image.LoadAsync(inputStream, cancellationToken);
            using var jpegStream = new MemoryStream();
            image.SaveAsJpeg(jpegStream);

            return new NormalizedInput(CreateSinglePagePdf(jpegStream.ToArray(), image.Width, image.Height), "invoice.pdf");
        }

        return new NormalizedInput(inputBytes, "invoice.pdf");
    }

    private static string NormalizeContentType(string contentType)
        => string.IsNullOrWhiteSpace(contentType) ? "application/pdf" : contentType.Split(';', 2)[0].Trim();

    private static string? NormalizeJsonContent(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        var normalized = content.Trim();
        if (normalized.StartsWith("```", StringComparison.Ordinal))
        {
            var firstNewLine = normalized.IndexOf('\n');
            if (firstNewLine >= 0)
            {
                normalized = normalized[(firstNewLine + 1)..];
            }

            normalized = normalized.Trim();
            if (normalized.EndsWith("```", StringComparison.Ordinal))
            {
                normalized = normalized[..^3];
            }
        }

        return normalized.Trim();
    }

    private static string ExtractAnnotationText(string responseContent)
    {
        using var rootDocument = JsonDocument.Parse(responseContent);
        var root = rootDocument.RootElement;
        var annotations = ExtractFileAnnotations(root);
        if (annotations.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var annotation in annotations)
        {
            if (string.IsNullOrWhiteSpace(annotation.Content))
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.AppendLine(annotation.Content);
        }

        return builder.ToString().Trim();
    }

    private static InvoiceExtractionResult MergeWithOcrFallback(InvoiceExtractionResult extractionResult, string ocrText)
    {
        var fallback = TryExtractFallbackFields(ocrText);

        return extractionResult with
        {
            SupplierName = extractionResult.SupplierName ?? fallback.SupplierName,
            SupplierEik = extractionResult.SupplierEik ?? fallback.SupplierEik,
            SupplierVatNumber = extractionResult.SupplierVatNumber ?? fallback.SupplierVatNumber,
            BuyerName = extractionResult.BuyerName ?? fallback.BuyerName,
            InvoiceNumber = extractionResult.InvoiceNumber ?? fallback.InvoiceNumber,
            InvoiceDate = extractionResult.InvoiceDate ?? fallback.InvoiceDate,
            Currency = extractionResult.Currency ?? fallback.Currency,
            NetTotal = extractionResult.NetTotal ?? fallback.NetTotal,
            VatTotal = extractionResult.VatTotal ?? fallback.VatTotal,
            GrossTotal = extractionResult.GrossTotal ?? fallback.GrossTotal,
            OverallConfidence = extractionResult.OverallConfidence ?? fallback.OverallConfidence
        };
    }

    private static OcrFallbackFields TryExtractFallbackFields(string ocrText)
    {
        var lines = ocrText
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        string? supplierName = null;
        string? supplierEik = null;
        string? supplierVatNumber = null;
        string? buyerName = null;

        foreach (var line in lines.Where(line => line.Contains('|')))
        {
            var cells = line
                .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(cell => !string.IsNullOrWhiteSpace(cell))
                .ToArray();

            if (cells.Length < 4)
            {
                continue;
            }

            for (var index = 0; index + 1 < cells.Length; index += 2)
            {
                var label = cells[index];
                var value = cells[index + 1];

                if (label.Contains("Получател", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(buyerName))
                {
                    buyerName = value;
                }
                else if (label.Contains("Доставчик", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(supplierName))
                {
                    supplierName = value;
                }
                else if (label.Equals("ЕИК", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(supplierEik))
                    {
                        supplierEik = value;
                    }
                }
                else if (label.Contains("ИН ДДС", StringComparison.OrdinalIgnoreCase) ||
                         label.Contains("ДДС", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(supplierVatNumber))
                    {
                        supplierVatNumber = value;
                    }
                }
            }
        }

        if (string.IsNullOrWhiteSpace(supplierVatNumber) && !string.IsNullOrWhiteSpace(supplierEik))
        {
            supplierVatNumber = $"BG{supplierEik}";
        }

        var invoiceNumber = TryRegexValue(
            ocrText,
            @"(?:Фактура|Invoice)\s*(?:№|No\.?|Number)?\s*[:\-]?\s*([A-Z0-9\-\/]+)");

        var invoiceDate = TryParseDate(
            TryRegexValue(
                ocrText,
                @"(?:Дата|Date)\s*[:\-]?\s*(\d{1,2}[./]\d{1,2}[./]\d{2,4})"));

        var currency = TryRegexValue(ocrText, @"\b(BGN|EUR|USD|лв\.?|лв)\b");
        if (string.Equals(currency, "лв", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(currency, "лв.", StringComparison.OrdinalIgnoreCase))
        {
            currency = "BGN";
        }

        var netTotal = TryParseDecimal(
            TryRegexValue(ocrText, @"(?:Общо\s*без\s*ДДС|Net\s*total|Subtotal|Subtotal\s*[:\-]?)\s*([0-9]+(?:[.,][0-9]+)?)"));
        var vatTotal = TryParseDecimal(
            TryRegexValue(ocrText, @"(?:ДДС|VAT)\s*([0-9]+(?:[.,][0-9]+)?)"));
        var grossTotal = TryParseDecimal(
            TryRegexValue(ocrText, @"(?:Общо|Total\s*amount|Grand\s*total)\s*([0-9]+(?:[.,][0-9]+)?)"));

        var overallConfidence = HasAnyFallbackValue(
            supplierName,
            supplierEik,
            supplierVatNumber,
            buyerName,
            invoiceNumber,
            invoiceDate,
            currency,
            netTotal,
            vatTotal,
            grossTotal)
            ? 0.95m
            : 0.0m;

        return new OcrFallbackFields(
            supplierName,
            supplierEik,
            supplierVatNumber,
            buyerName,
            invoiceNumber,
            invoiceDate,
            currency,
            netTotal,
            vatTotal,
            grossTotal,
            overallConfidence);
    }

    private static string? TryRegexValue(string input, string pattern)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            input,
            pattern,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline);
        return match.Success && match.Groups.Count > 1 ? match.Groups[1].Value.Trim() : null;
    }

    private static DateTimeOffset? TryParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateTimeOffset.TryParse(value, out var parsed) ? parsed : null;
    }

    private static decimal? TryParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Replace(" ", string.Empty).Replace(',', '.');
        return decimal.TryParse(normalized, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    private static bool HasAnyFallbackValue(params object?[] values)
        => values.Any(value => value is not null && (!value.Equals(string.Empty)));

    private static List<FileAnnotation> ExtractFileAnnotations(JsonElement root)
    {
        var annotations = new List<FileAnnotation>();
        var seenHashes = new HashSet<string>(StringComparer.Ordinal);

        if (root.TryGetProperty("choices", out var choicesElement) &&
            choicesElement.ValueKind == JsonValueKind.Array &&
            choicesElement.GetArrayLength() > 0)
        {
            var message = choicesElement[0].GetProperty("message");
            if (message.TryGetProperty("annotations", out var successAnnotations))
            {
                CollectAnnotations(successAnnotations, annotations, seenHashes);
            }
        }

        if (root.TryGetProperty("error", out var errorElement) &&
            errorElement.TryGetProperty("metadata", out var metadataElement) &&
            metadataElement.TryGetProperty("file_annotations", out var errorAnnotations))
        {
            CollectAnnotations(errorAnnotations, annotations, seenHashes);
        }

        return annotations;
    }

    private static void CollectAnnotations(
        JsonElement annotationsElement,
        ICollection<FileAnnotation> annotations,
        ISet<string> seenHashes)
    {
        if (annotationsElement.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var annotationElement in annotationsElement.EnumerateArray())
        {
            if (!annotationElement.TryGetProperty("type", out var typeElement) ||
                typeElement.ValueKind != JsonValueKind.String ||
                !string.Equals(typeElement.GetString(), "file", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!annotationElement.TryGetProperty("file", out var fileElement) ||
                !fileElement.TryGetProperty("hash", out var hashElement) ||
                hashElement.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            var hash = hashElement.GetString();
            if (string.IsNullOrWhiteSpace(hash) || !seenHashes.Add(hash))
            {
                continue;
            }

            var contentText = string.Empty;
            if (fileElement.TryGetProperty("content", out var contentElement) &&
                contentElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var part in contentElement.EnumerateArray())
                {
                    if (part.TryGetProperty("type", out var partType) &&
                        partType.ValueKind == JsonValueKind.String &&
                        string.Equals(partType.GetString(), "text", StringComparison.OrdinalIgnoreCase) &&
                        part.TryGetProperty("text", out var textElement) &&
                        textElement.ValueKind == JsonValueKind.String)
                    {
                        contentText += textElement.GetString();
                    }
                }
            }

            annotations.Add(new FileAnnotation(hash, contentText));
        }
    }

    private static byte[] CreateSinglePagePdf(byte[] jpegBytes, int width, int height)
    {
        using var pdfStream = new MemoryStream();
        var offsets = new List<long>(capacity: 5);

        WriteAscii(pdfStream, "%PDF-1.4\n");
        WriteLatin1(pdfStream, "%âãÏÓ\n");

        WriteTextObject(pdfStream, offsets, 1, "<< /Type /Catalog /Pages 2 0 R >>\n");
        WriteTextObject(pdfStream, offsets, 2, "<< /Type /Pages /Count 1 /Kids [3 0 R] >>\n");
        WriteTextObject(
            pdfStream,
            offsets,
            3,
            $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {width} {height}] /Resources << /XObject << /Im0 5 0 R >> >> /Contents 4 0 R >>\n");

        var contentBytes = Encoding.ASCII.GetBytes($"q\n{width} 0 0 {height} 0 0 cm\n/Im0 Do\nQ\n");
        WriteStreamObject(pdfStream, offsets, 4, $"<< /Length {contentBytes.Length} >>\n", contentBytes);
        WriteStreamObject(
            pdfStream,
            offsets,
            5,
            $"<< /Type /XObject /Subtype /Image /Width {width} /Height {height} /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length {jpegBytes.Length} >>\n",
            jpegBytes);

        var xrefPosition = pdfStream.Position;
        WriteAscii(pdfStream, $"xref\n0 {offsets.Count + 1}\n");
        WriteAscii(pdfStream, "0000000000 65535 f \n");
        foreach (var offset in offsets)
        {
            WriteAscii(pdfStream, $"{offset:0000000000} 00000 n \n");
        }

        WriteAscii(pdfStream, $"trailer\n<< /Size {offsets.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefPosition}\n%%EOF");
        return pdfStream.ToArray();
    }

    private static void WriteTextObject(Stream stream, List<long> offsets, int objectId, string body)
    {
        offsets.Add(stream.Position);
        WriteAscii(stream, $"{objectId} 0 obj\n{body}endobj\n");
    }

    private static void WriteStreamObject(Stream stream, List<long> offsets, int objectId, string header, byte[] data)
    {
        offsets.Add(stream.Position);
        WriteAscii(stream, $"{objectId} 0 obj\n{header}stream\n");
        stream.Write(data, 0, data.Length);
        WriteAscii(stream, "\nendstream\nendobj\n");
    }

    private static void WriteAscii(Stream stream, string value)
    {
        var bytes = Encoding.ASCII.GetBytes(value);
        stream.Write(bytes, 0, bytes.Length);
    }

    private static void WriteLatin1(Stream stream, string value)
    {
        var bytes = Encoding.Latin1.GetBytes(value);
        stream.Write(bytes, 0, bytes.Length);
    }

    private sealed record NormalizedInput(byte[] Content, string FileName);

    private sealed record OpenRouterChatCompletionResponse
    {
        public OpenRouterChoice[] Choices { get; init; } = [];
    }

    private sealed record OpenRouterChoice
    {
        public OpenRouterMessage? Message { get; init; }
    }

    private sealed record OpenRouterMessage
    {
        public string? Content { get; init; }
    }

    private sealed record FileAnnotation(string Hash, string Content);

    private sealed record OcrFallbackFields(
        string? SupplierName,
        string? SupplierEik,
        string? SupplierVatNumber,
        string? BuyerName,
        string? InvoiceNumber,
        DateTimeOffset? InvoiceDate,
        string? Currency,
        decimal? NetTotal,
        decimal? VatTotal,
        decimal? GrossTotal,
        decimal? OverallConfidence);
}
