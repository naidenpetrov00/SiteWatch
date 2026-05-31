using Domain.Entities;

namespace Application.SeedWork.Interfaces;

public interface IInvoiceValidationService
{
    Task<List<InvoiceReviewIssue>> ValidateAsync(
        InvoiceDocument invoiceDocument,
        CancellationToken cancellationToken = default);
}
