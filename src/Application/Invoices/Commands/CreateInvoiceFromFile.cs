using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using MediatR;

namespace Application.Invoices.Commands;

[Authorize(Roles = UserRoleGroups.AdministratorOrWorker)]
public sealed record CreateInvoiceFromFileCommand(Guid SiteId, UploadedInvoiceFile File)
    : IRequest<Guid>;

public sealed class CreateInvoiceFromFileCommandHandler(
    IInvoiceService invoiceService,
    IInvoiceBlobService invoiceBlobService,
    IUser user)
    : IRequestHandler<CreateInvoiceFromFileCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateInvoiceFromFileCommand request,
        CancellationToken cancellationToken)
    {
        var userId = user.Id ?? throw new UnauthorizedAccessException();
        await invoiceService.EnsureUserCanAccessSiteAsync(
            request.SiteId,
            userId,
            cancellationToken);

        await using var validatedFile = await InvoiceFileValidation.ValidateAndBufferAsync(
            request.File,
            cancellationToken);
        var invoiceId = Guid.NewGuid();

        await invoiceBlobService.UploadAsync(invoiceId, validatedFile, cancellationToken);

        try
        {
            await invoiceService.CreateIncompleteAsync(
                invoiceId,
                request.SiteId,
                userId,
                cancellationToken);
        }
        catch
        {
            await invoiceBlobService.DeleteIfExistsAsync(invoiceId, CancellationToken.None);
            throw;
        }

        return invoiceId;
    }
}
