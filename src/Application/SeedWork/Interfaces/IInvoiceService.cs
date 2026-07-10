using Application.Invoices.Queries;
using Application.SeedWork.Models;

namespace Application.SeedWork.Interfaces;

public interface IInvoiceService
{
    Task<PagedResult<DashboardInvoiceDto>> GetDashboardInvoicesAsync(
        DashboardInvoicesQuery request,
        CancellationToken cancellationToken
    );
}
