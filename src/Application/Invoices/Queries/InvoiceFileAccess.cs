using Application.SeedWork.Interfaces;
using Application.SeedWork.Exceptions;
using Application.SeedWork.Models.Internal;
using Application.SeedWork.Security;
using MediatR;

namespace Application.Invoices.Queries;

/// <summary>Provides temporary read access to an invoice file.</summary>
public sealed record InvoiceFileAccessDto(
    string Url,
    string FileName,
    string ContentType,
    DateTimeOffset ExpiresAt);

public sealed record InvoiceFileInfoDto(
    string FileName,
    string ContentType);

[Authorize(Roles = UserRoleGroups.AdministratorOrWorker)]
public sealed record InvoiceFileAccessQuery(Guid SiteId, Guid InvoiceId)
    : IRequest<InvoiceFileInfoDto>;

public sealed class InvoiceFileAccessQueryHandler(
    IInvoiceService invoiceService,
    IInvoiceBlobService invoiceBlobService,
    IUser user)
    : IRequestHandler<InvoiceFileAccessQuery, InvoiceFileInfoDto>
{
    public async Task<InvoiceFileInfoDto> Handle(
        InvoiceFileAccessQuery request,
        CancellationToken cancellationToken)
    {
        await invoiceService.EnsureUserCanAccessInvoiceAsync(
            request.SiteId,
            request.InvoiceId,
            user.Id ?? throw new UnauthorizedAccessException(),
            cancellationToken);

        return await invoiceBlobService.GetInfoAsync(
            request.InvoiceId,
            cancellationToken);
    }
}

public sealed record InvoiceFileDownloadQuery(
    Guid SiteId,
    Guid InvoiceId,
    string UserId) : IRequest<InvoiceFileResponse>;

public sealed class InvoiceFileDownloadQueryHandler(
    IIdentityService identityService,
    IInvoiceService invoiceService,
    IInvoiceBlobService invoiceBlobService)
    : IRequestHandler<InvoiceFileDownloadQuery, InvoiceFileResponse>
{
    public async Task<InvoiceFileResponse> Handle(
        InvoiceFileDownloadQuery request,
        CancellationToken cancellationToken)
    {
        var isAdministrator = await identityService.IsInRoleAsync(
            request.UserId,
            UserRoles.Administrator);
        var isWorker = await identityService.IsInRoleAsync(
            request.UserId,
            UserRoles.Worker);

        if (!isAdministrator && !isWorker)
        {
            throw new ForbiddenAccessException();
        }

        await invoiceService.EnsureUserCanAccessInvoiceAsync(
            request.SiteId,
            request.InvoiceId,
            request.UserId,
            cancellationToken);

        return await invoiceBlobService.DownloadAsync(
            request.InvoiceId,
            cancellationToken);
    }
}
