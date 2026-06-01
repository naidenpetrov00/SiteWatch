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
                    Return confidence in extracted values between 0 and 1 so that if there is missunderstood answer i will review and update it.
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
                            Return a single JSON object with this shape:
                            {
                              "documentType": "Invoice|Receipt|Offer|Unknown",
                              "documentTypeConfidence": 0.0,
                              "supplierName": "string|null",
                              "supplierNameConfidence": 0.0,
                              "supplierEik": "string|null",
                              "supplierEikConfidence": 0.0,
                              "supplierVatNumber": "string|null",
                              "supplierVatNumberConfidence": 0.0,
                              "buyerName": "string|null",
                              "buyerNameConfidence": 0.0,
                              "invoiceNumber": "string|null",
                              "invoiceNumberConfidence": 0.0,
                              "invoiceDate": "ISO-8601 string|null",
                              "invoiceDateConfidence": 0.0,
                              "currency": "string|null",
                              "currencyConfidence": 0.0,
                              "netTotal": 0.0,
                              "netTotalConfidence": 0.0,
                              "vatTotal": 0.0,
                              "vatTotalConfidence": 0.0,
                              "grossTotal": 0.0,
                              "grossTotalConfidence": 0.0,
                              "overallConfidence": 0.0,
                              "items": [
                                {
                                  "productCode": "string|null",
                                  "productName": "string|null",
                                  "quantity": 0.0,
                                  "unit": "string|null",
                                  "unitPrice": 0.0,
                                  "discount": 0.0,
                                  "vatRate": 0.0,
                                  "lineTotal": 0.0,
                                  "confidence": 0.0
                                }
                              ],
                              "issues": [
                                {
                                  "fieldPath": "string",
                                  "extractedValue": "string|null",
                                  "reason": "string",
                                  "confidence": 0.0
                                }
                              ]
                            }

                            Rules:
                            - documentType must be one of Invoice, Receipt, Offer, Unknown.
                            - Use null for missing values.
                            - Do not infer values that are not explicitly supported by the document.
                            - Preserve original product names and descriptions.
                            - Include all visible product lines.
                            - Flag suspicious totals, missing identifiers, unclear dates, and ambiguous supplier or buyer data in issues.
                            - If a value is uncertain, prefer null and add an issue instead of guessing.
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
            response_format = new
            {
                type = "json_schema",
                json_schema = new
                {
                    name = "invoice_extraction_result",
                    strict = true,
                    schema = BuildInvoiceExtractionSchema()
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

    private static InvoiceExtractionResult? ParseExtractionResult(string responseContent)
    {
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

    private static object BuildInvoiceExtractionSchema()
        => new
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
                "issues"
            },
            properties = new
            {
                documentType = new { type = "string", @enum = new[] { "Invoice", "Receipt", "Offer", "Unknown" } },
                documentTypeConfidence = NullableNumberSchema(),
                supplierName = NullableStringSchema(),
                supplierNameConfidence = NullableNumberSchema(),
                supplierEik = NullableStringSchema(),
                supplierEikConfidence = NullableNumberSchema(),
                supplierVatNumber = NullableStringSchema(),
                supplierVatNumberConfidence = NullableNumberSchema(),
                buyerName = NullableStringSchema(),
                buyerNameConfidence = NullableNumberSchema(),
                invoiceNumber = NullableStringSchema(),
                invoiceNumberConfidence = NullableNumberSchema(),
                invoiceDate = NullableDateTimeStringSchema(),
                invoiceDateConfidence = NullableNumberSchema(),
                currency = NullableStringSchema(),
                currencyConfidence = NullableNumberSchema(),
                netTotal = NullableNumberSchema(),
                netTotalConfidence = NullableNumberSchema(),
                vatTotal = NullableNumberSchema(),
                vatTotalConfidence = NullableNumberSchema(),
                grossTotal = NullableNumberSchema(),
                grossTotalConfidence = NullableNumberSchema(),
                overallConfidence = NullableNumberSchema(),
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
                            productCode = NullableStringSchema(),
                            productName = NullableStringSchema(),
                            quantity = NullableNumberSchema(),
                            unit = NullableStringSchema(),
                            unitPrice = NullableNumberSchema(),
                            discount = NullableNumberSchema(),
                            vatRate = NullableNumberSchema(),
                            lineTotal = NullableNumberSchema(),
                            confidence = NullableNumberSchema()
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
                        required = new[]
                        {
                            "fieldPath",
                            "extractedValue",
                            "reason",
                            "confidence"
                        },
                        properties = new
                        {
                            fieldPath = new { type = "string" },
                            extractedValue = NullableStringSchema(),
                            reason = new { type = "string" },
                            confidence = NullableNumberSchema()
                        }
                    }
                }
            }
        };

    private static object NullableStringSchema() => new { type = new[] { "string", "null" } };

    private static object NullableNumberSchema() => new { type = new[] { "number", "null" } };

    private static object NullableDateTimeStringSchema() => new { type = new[] { "string", "null" }, format = "date-time" };

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
