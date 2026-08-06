using Application.Invoices.Commands;
using Application.Invoices.Queries;
using Application.SeedWork.Models;

namespace Application.SeedWork.Interfaces;

public interface IInvoiceService
{
    Task<Guid> CreateAsync(CreateInvoiceCommand request, CancellationToken cancellationToken);

    Task CreateIncompleteAsync(
        Guid invoiceId,
        Guid siteId,
        string userId,
        CancellationToken cancellationToken);

    Task UpdateAsync(UpdateInvoiceCommand request, CancellationToken cancellationToken);

    Task UpdateSiteAllocationsAsync(
        UpdateInvoiceSiteAllocationsCommand request,
        CancellationToken cancellationToken);

    Task<PagedResult<DashboardInvoiceDto>> GetDashboardInvoicesAsync(
        DashboardInvoicesQuery request,
        CancellationToken cancellationToken
    );

    Task<IReadOnlyList<SiteInvoiceDto>> GetSiteInvoicesAsync(
        Guid siteId,
        string userId,
        CancellationToken cancellationToken);

    Task EnsureUserCanAccessInvoiceAsync(
        Guid siteId,
        Guid invoiceId,
        string userId,
        CancellationToken cancellationToken);

    Task EnsureInvoiceExistsAsync(Guid invoiceId, CancellationToken cancellationToken);

    Task EnsureUserCanAccessSiteAsync(
        Guid siteId,
        string userId,
        CancellationToken cancellationToken);
}
