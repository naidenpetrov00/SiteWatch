using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.SeedWork.Converters;
using Application.SeedWork.Exceptions;
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
        var ocrResponseContent = await SendOcrRequestAsync(
            apiKey,
            model,
            parserEngine,
            normalizedFile.FileName,
            pdfDataUrl,
            cancellationToken);

        var rawOcrText = ExtractAnnotationText(ocrResponseContent);
        if (string.IsNullOrWhiteSpace(rawOcrText))
        {
            throw new OpenRouterInvoiceExtractionException(
                "OpenRouter returned no OCR text annotations for the uploaded document.",
                ocrResponseContent);
        }

        logger.LogDebug(
            "OpenRouter OCR text preview: {Preview}",
            rawOcrText.Length > 800 ? rawOcrText[..800] : rawOcrText);

        var extractionResponseContent = await SendExtractionRequestAsync(
            apiKey,
            model,
            rawOcrText,
            cancellationToken);

        return ParseExtractionResult(extractionResponseContent, rawOcrText);
    }

    private async Task<string> SendOcrRequestAsync(
        string apiKey,
        string model,
        string parserEngine,
        string fileName,
        string pdfDataUrl,
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
                    You extract OCR text from invoice-like documents.
                    Return only the text extracted from the uploaded file.
                    Do not infer values or normalize the content.
                    Preserve every visible amount and currency marker exactly as it appears.
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
                            text = "Extract the OCR text from this document."
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
            response_format = new
            {
                type = "json_object"
            },
            stream = false
        };

        return await SendRequestAsync(apiKey, requestBody, null, cancellationToken);
    }

    private async Task<string> SendExtractionRequestAsync(
        string apiKey,
        string model,
        string rawOcrText,
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
                    Use only values that are explicitly present in the provided OCR text.
                    Do not guess, infer, normalize, or fabricate any value.
                    If a field is not clearly present, return null.
                    The document may be in Bulgarian. Interpret common labels correctly.
                    If both BGN and EUR values are visible, extract both and keep them separate.
                    Do not collapse converted amounts into a single currency.
                    Preserve the visible primary amount in the legacy field and also populate the BGN and EUR amount fields when present.
                    Return confidence in every extracted value between 0 and 1.
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
                            Use only values that are present in the OCR text.
                            If both BGN and EUR are present on the document, populate both amount fields end to end.
                            Prefer null over guessing when one side of the currency pair is missing.
                            """
                        },
                        new
                        {
                            type = "text",
                            text = rawOcrText
                        }
                    }
                }
            },
            response_format = BuildResponseFormat(),
            stream = false
        };

        return await SendRequestAsync(apiKey, requestBody, rawOcrText, cancellationToken);
    }

    private async Task<string> SendRequestAsync(
        string apiKey,
        object requestBody,
        string? rawOcrText,
        CancellationToken cancellationToken)
    {
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
                (int)response.StatusCode,
                rawOcrText: rawOcrText);
        }

        return responseContent;
    }

    private static object BuildResponseFormat()
        => new
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
                        "netTotalBgn",
                        "netTotalBgnConfidence",
                        "netTotalEur",
                        "netTotalEurConfidence",
                        "netTotalConfidence",
                        "vatTotal",
                        "vatTotalBgn",
                        "vatTotalBgnConfidence",
                        "vatTotalEur",
                        "vatTotalEurConfidence",
                        "vatTotalConfidence",
                        "grossTotal",
                        "grossTotalBgn",
                        "grossTotalBgnConfidence",
                        "grossTotalEur",
                        "grossTotalEurConfidence",
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
                        netTotalBgn = new { type = new[] { "number", "null" } },
                        netTotalBgnConfidence = new { type = new[] { "number", "null" } },
                        netTotalEur = new { type = new[] { "number", "null" } },
                        netTotalEurConfidence = new { type = new[] { "number", "null" } },
                        netTotalConfidence = new { type = new[] { "number", "null" } },
                        vatTotal = new { type = new[] { "number", "null" } },
                        vatTotalBgn = new { type = new[] { "number", "null" } },
                        vatTotalBgnConfidence = new { type = new[] { "number", "null" } },
                        vatTotalEur = new { type = new[] { "number", "null" } },
                        vatTotalEurConfidence = new { type = new[] { "number", "null" } },
                        vatTotalConfidence = new { type = new[] { "number", "null" } },
                        grossTotal = new { type = new[] { "number", "null" } },
                        grossTotalBgn = new { type = new[] { "number", "null" } },
                        grossTotalBgnConfidence = new { type = new[] { "number", "null" } },
                        grossTotalEur = new { type = new[] { "number", "null" } },
                        grossTotalEurConfidence = new { type = new[] { "number", "null" } },
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
                                    "unitPriceBgn",
                                    "unitPriceBgnConfidence",
                                    "unitPriceEur",
                                    "unitPriceEurConfidence",
                                    "discount",
                                    "discountBgn",
                                    "discountBgnConfidence",
                                    "discountEur",
                                    "discountEurConfidence",
                                    "vatRate",
                                    "lineTotal",
                                    "lineTotalBgn",
                                    "lineTotalBgnConfidence",
                                    "lineTotalEur",
                                    "lineTotalEurConfidence",
                                    "confidence"
                                },
                                properties = new
                                {
                                    productCode = new { type = new[] { "string", "null" } },
                                    productName = new { type = new[] { "string", "null" } },
                                    quantity = new { type = new[] { "number", "null" } },
                                    unit = new { type = new[] { "string", "null" } },
                                    unitPrice = new { type = new[] { "number", "null" } },
                                    unitPriceBgn = new { type = new[] { "number", "null" } },
                                    unitPriceBgnConfidence = new { type = new[] { "number", "null" } },
                                    unitPriceEur = new { type = new[] { "number", "null" } },
                                    unitPriceEurConfidence = new { type = new[] { "number", "null" } },
                                    discount = new { type = new[] { "number", "null" } },
                                    discountBgn = new { type = new[] { "number", "null" } },
                                    discountBgnConfidence = new { type = new[] { "number", "null" } },
                                    discountEur = new { type = new[] { "number", "null" } },
                                    discountEurConfidence = new { type = new[] { "number", "null" } },
                                    vatRate = new { type = new[] { "number", "null" } },
                                    lineTotal = new { type = new[] { "number", "null" } },
                                    lineTotalBgn = new { type = new[] { "number", "null" } },
                                    lineTotalBgnConfidence = new { type = new[] { "number", "null" } },
                                    lineTotalEur = new { type = new[] { "number", "null" } },
                                    lineTotalEurConfidence = new { type = new[] { "number", "null" } },
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
        };

    private InvoiceExtractionResult? ParseExtractionResult(string responseContent, string? rawOcrText = null)
    {
        using var rootDocument = JsonDocument.Parse(responseContent);
        var root = rootDocument.RootElement;

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
                providerCode,
                rawOcrText: rawOcrText);
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
                innerException: ex,
                rawOcrText: rawOcrText);
        }

        if (completion is null)
        {
            throw new OpenRouterInvoiceExtractionException(
                "OpenRouter returned an empty extraction payload.",
                responseContent,
                rawOcrText: rawOcrText);
        }

        var content = NormalizeJsonContent(completion.Choices.FirstOrDefault()?.Message?.Content);
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new OpenRouterInvoiceExtractionException(
                "OpenRouter returned an empty extraction payload.",
                responseContent,
                rawOcrText: rawOcrText);
        }

        try
        {
            var extractionResult = JsonSerializer.Deserialize<InvoiceExtractionResult>(content, JsonSerializerOptions);
            if (extractionResult is null)
            {
                throw new OpenRouterInvoiceExtractionException(
                    "OpenRouter returned an empty extraction payload.",
                    responseContent,
                    rawOcrText: rawOcrText);
            }

            return extractionResult with
            {
                RawJson = content,
                RawOcrText = rawOcrText
            };
        }
        catch (JsonException ex)
        {
            throw new OpenRouterInvoiceExtractionException(
                "OpenRouter returned invalid invoice JSON.",
                responseContent,
                innerException: ex,
                rawOcrText: rawOcrText);
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
}
