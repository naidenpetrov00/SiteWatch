using Application.SeedWork.Models.External;

namespace Application.SeedWork.Interfaces;

public interface IInvoiceExtractor
{
    Task<InvoiceExtractionResult?> ExtractAsync(
        Stream stream,
        string contentType,
        CancellationToken cancellationToken = default);
}
