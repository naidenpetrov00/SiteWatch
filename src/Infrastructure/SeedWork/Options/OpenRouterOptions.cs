namespace Infrastructure.SeedWork.Options;

public class OpenRouterOptions
{
    public string? ApiKey { get; set; }

    public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1";

    public string Model { get; set; } = "google/gemma-4-26b-a4b-it";

    public string PdfParserEngine { get; set; } = "mistral-ocr";
}
