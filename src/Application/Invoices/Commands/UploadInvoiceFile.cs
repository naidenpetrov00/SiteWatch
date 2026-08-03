using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using MediatR;

namespace Application.Invoices.Commands;

public sealed record UploadedInvoiceFile(Stream Stream, string FileName, string ContentType);

[Authorize(Roles = UserRoles.Administrator)]
public sealed record UploadInvoiceFileCommand(Guid InvoiceId, UploadedInvoiceFile File) : IRequest;

public sealed class UploadInvoiceFileCommandHandler(
    IInvoiceService invoiceService,
    IInvoiceBlobService invoiceBlobService)
    : IRequestHandler<UploadInvoiceFileCommand>
{
    public async Task Handle(
        UploadInvoiceFileCommand request,
        CancellationToken cancellationToken)
    {
        await invoiceService.EnsureInvoiceExistsAsync(request.InvoiceId, cancellationToken);
        await invoiceBlobService.UploadAsync(
            request.InvoiceId,
            request.File,
            cancellationToken);
    }
}
