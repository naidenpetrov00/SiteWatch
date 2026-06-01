using Application.SeedWork.Models.External;

namespace Application.SeedWork.Interfaces;

public interface IInvoiceValidationService
{
    Task<InvoiceValidationResult> ValidateAsync(
        InvoiceExtractionResult invoiceExtractionResult,
        CancellationToken cancellationToken = default);
}
