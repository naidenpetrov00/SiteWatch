using Application.SeedWork.Interfaces;
using Domain.Entities;

namespace Infrastructure.InvoiceExtraction;

internal sealed class OpenRouterInvoiceExtractor : IInvoiceExtractor
{
    public Task<InvoiceDocument?> ExtractAsync(
        Stream stream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<InvoiceDocument?>(null);
    }
}
