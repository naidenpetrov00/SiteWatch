using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.External;
using Ardalis.GuardClauses;
using Infrastructure.SeedWork.Options;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.InvoiceExtraction;

public sealed class OpenRouterInvoiceExtractor : IInvoiceExtractor
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
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
        var mimeType = string.IsNullOrWhiteSpace(contentType)
            ? "application/pdf"
            : contentType.Split(';', 2)[0].Trim();

        await using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);
        var pdfDataUrl = $"data:{mimeType};base64,{Convert.ToBase64String(memoryStream.ToArray())}";

        var requestBody = new
        {
            model,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = "Extract invoice data from the attached PDF. Return only valid JSON and use null for values that cannot be determined."
                },
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "text",
                            text = "Extract the invoice into JSON with the following fields: DocumentType, SupplierName, SupplierEik, SupplierVatNumber, BuyerName, InvoiceNumber, InvoiceDate, Currency, NetTotal, VatTotal, GrossTotal, OverallConfidence, Items, Issues. DocumentType should be Pdf for PDF invoices. InvoiceDate must be ISO 8601. Items must include ProductCode, ProductName, Quantity, Unit, UnitPrice, Discount, VatRate, LineTotal, Confidence. Issues must include FieldPath, ExtractedValue, Reason, Confidence."
                        },
                        new
                        {
                            type = "file",
                            file = new
                            {
                                filename = "invoice.pdf",
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

        var content = completion?.Choices.FirstOrDefault()?.Message?.Content;
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        var extractionResult = JsonSerializer.Deserialize<InvoiceExtractionResult>(content, JsonSerializerOptions);
        return extractionResult is null ? null : extractionResult with { RawJson = content };
    }

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
