using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.External;

namespace Infrastructure.InvoiceExtraction;

internal sealed class OpenRouterInvoiceExtractor : IInvoiceExtractor
{
    public Task<InvoiceExtractionResult?> ExtractAsync(
        Stream stream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<InvoiceExtractionResult?>(null);
    }
}
