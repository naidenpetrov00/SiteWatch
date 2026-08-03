using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using MediatR;

namespace Application.Invoices.Queries;

/// <summary>Provides temporary read access to an invoice file.</summary>
public sealed record InvoiceFileAccessDto(
    string Url,
    string FileName,
    string ContentType,
    DateTimeOffset ExpiresAt);

[Authorize(Roles = UserRoleGroups.AdministratorOrWorker)]
public sealed record InvoiceFileAccessQuery(Guid SiteId, Guid InvoiceId)
    : IRequest<InvoiceFileAccessDto>;

public sealed class InvoiceFileAccessQueryHandler(
    IInvoiceService invoiceService,
    IInvoiceBlobService invoiceBlobService,
    IUser user)
    : IRequestHandler<InvoiceFileAccessQuery, InvoiceFileAccessDto>
{
    private static readonly TimeSpan AccessLifetime = TimeSpan.FromMinutes(5);

    public async Task<InvoiceFileAccessDto> Handle(
        InvoiceFileAccessQuery request,
        CancellationToken cancellationToken)
    {
        await invoiceService.EnsureUserCanAccessInvoiceAsync(
            request.SiteId,
            request.InvoiceId,
            user.Id ?? throw new UnauthorizedAccessException(),
            cancellationToken);

        return await invoiceBlobService.CreateReadAccessAsync(
            request.InvoiceId,
            AccessLifetime,
            cancellationToken);
    }
}
