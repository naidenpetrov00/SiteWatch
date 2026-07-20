using Application.Invoices.Commands;
using Application.Invoices.Queries;
using Application.SeedWork.Models;

namespace Application.SeedWork.Interfaces;

public interface IInvoiceService
{
    Task<Guid> CreateAsync(CreateInvoiceCommand request, CancellationToken cancellationToken);

    Task<PagedResult<DashboardInvoiceDto>> GetDashboardInvoicesAsync(
        DashboardInvoicesQuery request,
        CancellationToken cancellationToken
    );
}
