using Application.Invoices.Commands;
using Application.Invoices.Queries;

namespace Application.SeedWork.Interfaces;

public interface IInvoiceBlobService
{
    Task UploadAsync(
        Guid invoiceId,
        UploadedInvoiceFile file,
        CancellationToken cancellationToken = default);

    Task<InvoiceFileAccessDto> CreateReadAccessAsync(
        Guid invoiceId,
        TimeSpan lifetime,
        CancellationToken cancellationToken = default);
}
