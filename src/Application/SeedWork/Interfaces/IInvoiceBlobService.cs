using Application.Invoices.Commands;
using Application.Invoices.Queries;
using Application.SeedWork.Models.Internal;

namespace Application.SeedWork.Interfaces;

public interface IInvoiceBlobService
{
    Task UploadAsync(
        Guid invoiceId,
        UploadedInvoiceFile file,
        CancellationToken cancellationToken = default);

    Task DeleteIfExistsAsync(
        Guid invoiceId,
        CancellationToken cancellationToken = default);

    Task<InvoiceFileInfoDto> GetInfoAsync(
        Guid invoiceId,
        CancellationToken cancellationToken = default);

    Task<InvoiceFileResponse> DownloadAsync(
        Guid invoiceId,
        CancellationToken cancellationToken = default);
}
