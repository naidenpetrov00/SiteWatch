using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
                    Return only one JSON object and no markdown, prose, or code fences.
                    Do not guess unclear values. Use null for anything missing, ambiguous, or unreadable.
                    Preserve product names exactly as written.
                    Extract every visible product or service line.
                    Detect the document type as Invoice, Receipt, Offer, or Unknown.
                    Add issues for unclear, suspicious, or contradictory fields.
                    Set confidence values between 0 and 1 when possible.
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
                            - Preserve the original product names and descriptions.
                            - Include all visible product lines.
                            - Flag suspicious totals, missing identifiers, unclear dates, and ambiguous supplier or buyer data in issues.
                            - If a value is uncertain, prefer null and add an issue instead of guessing.
                            - Use the OpenRouter file parser output and the configured model to fill the schema above.
                            """
                        },
                        new
                        {
                            type = "file",
                            file = new
                            {
                                filename = normalizedFile.FileName,
                                file_data = pdfDataUrl
                            }
                        }
                    }
                }
            },
            response_format = new
            {
                type = "json_object"
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
            throw new InvalidOperationException(
                $"OpenRouter invoice extraction failed with status code {(int)response.StatusCode}: {responseContent}");
        }

        var completion = JsonSerializer.Deserialize<OpenRouterChatCompletionResponse>(
            responseContent,
            JsonSerializerOptions);

        var content = NormalizeJsonContent(completion?.Choices.FirstOrDefault()?.Message?.Content);
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        var extractionResult = JsonSerializer.Deserialize<InvoiceExtractionResult>(content, JsonSerializerOptions);
        return extractionResult is null ? null : extractionResult with { RawJson = content };
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
            using var image = await Image.LoadAsync(inputBytes, cancellationToken);
            using var jpegStream = new MemoryStream();
            image.SaveAsJpeg(jpegStream);

            var pdfBytes = CreateSinglePagePdf(jpegStream.ToArray(), image.Width, image.Height);
            return new NormalizedInput(pdfBytes, "invoice.pdf");
        }

        return new NormalizedInput(inputBytes, "invoice.pdf");
    }

    private static string NormalizeContentType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return "application/pdf";
        }

        return contentType.Split(';', 2)[0].Trim();
    }

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

        var contentBytes = Encoding.ASCII.GetBytes(
            $"q\n{width} 0 0 {height} 0 0 cm\n/Im0 Do\nQ\n");
        WriteStreamObject(
            pdfStream,
            offsets,
            4,
            $"<< /Length {contentBytes.Length} >>\n",
            contentBytes);

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
