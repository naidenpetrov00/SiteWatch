using Domain.Entities;

namespace Application.SeedWork.Interfaces;

public interface IInvoiceExtractor
{
    Task<InvoiceDocument?> ExtractAsync(
        Stream stream,
        string contentType,
        CancellationToken cancellationToken = default);
}
