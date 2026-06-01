namespace Infrastructure.SeedWork.Options;

public class OpenRouterOptions
{
    public string? ApiKey { get; set; }

    public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1";

    public string Model { get; set; } = "deepseek/deepseek-v3.2";

    public string PdfParserEngine { get; set; } = "mistral-ocr";
}
