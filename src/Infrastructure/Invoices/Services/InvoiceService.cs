using Application.Invoices.Queries;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Application.SeedWork.Queries;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Invoices.Services;

public sealed class InvoiceService(ApplicationDbContext dbContext, IMapper mapper) : IInvoiceService
{
    public async Task<PagedResult<DashboardInvoiceDto>> GetDashboardInvoicesAsync(
        DashboardInvoicesQuery request,
        CancellationToken cancellationToken
    )
    {
        return await dbContext.Invoices
            .AsNoTracking()
            .ToPagedResultAsync<Domain.Entities.Invoice, DashboardInvoiceDto, DashboardInvoicesQuery>(
                request,
                DashboardInvoicesQuery.Table,
                query => query.ProjectTo<DashboardInvoiceDto>(mapper.ConfigurationProvider),
                cancellationToken
            );
    }
}
