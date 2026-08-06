using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using MediatR;

namespace Application.Invoices.Queries;

[Authorize(Roles = UserRoleGroups.AdministratorOrWorker)]
public sealed record SiteInvoicesQuery(Guid SiteId) : IRequest<IReadOnlyList<SiteInvoiceDto>>;

public sealed class SiteInvoicesQueryHandler(IInvoiceService invoiceService, IUser user)
    : IRequestHandler<SiteInvoicesQuery, IReadOnlyList<SiteInvoiceDto>>
{
    public Task<IReadOnlyList<SiteInvoiceDto>> Handle(
        SiteInvoicesQuery request,
        CancellationToken cancellationToken) =>
        invoiceService.GetSiteInvoicesAsync(
            request.SiteId,
            user.Id ?? throw new UnauthorizedAccessException(),
            cancellationToken);
}
