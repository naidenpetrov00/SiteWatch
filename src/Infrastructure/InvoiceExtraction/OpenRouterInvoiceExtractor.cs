using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.SeedWork.Exceptions;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.External;
using Ardalis.GuardClauses;
using Infrastructure.SeedWork.Options;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;

namespace Infrastructure.InvoiceExtraction;

public sealed class OpenRouterInvoiceExtractor : IInvoiceExtractor
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    private readonly HttpClient httpClient;
    private readonly OpenRouterOptions openRouterOptions;

    public OpenRouterInvoiceExtractor(HttpClient httpClient, IConfiguration configuration)
    {
        this.httpClient = httpClient;
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
        var pdfDataUrl = $"data:application/pdf;base64,{Convert.ToBase64String(normalizedFile.Content)}";

        var responseContent = await SendExtractionRequestAsync(
            apiKey,
            model,
            parserEngine,
            normalizedFile.FileName,
            pdfDataUrl,
            cancellationToken);

        return ParseExtractionResult(responseContent);
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
                    Do not guess unclear values. Use null for missing or unreadable values.
                    Preserve product names exactly as written.
                    Extract every visible product or service line.
                    Detect document type as Invoice, Receipt, Offer, or Unknown.
                    Add issues for unclear, suspicious, or contradictory fields.
                    Return confidence in extracted values between 0 and 1.
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
                            Extract the document into the response schema.
                            Missing values must be null.
                            Do not guess.
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
                },
                new
                {
                    id = "response-healing"
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

    private static InvoiceExtractionResult? ParseExtractionResult(string responseContent)
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
            return extractionResult is null ? null : extractionResult with { RawJson = content };
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
}
