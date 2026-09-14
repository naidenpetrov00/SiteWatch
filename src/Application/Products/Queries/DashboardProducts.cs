using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Application.SeedWork.Queries;
using Application.SeedWork.Security;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Products.Queries;

/// <summary>Requests a filtered, sorted page of product catalog entries.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed partial class DashboardProductsQuery
    : TableQueryRequest, IRequest<PagedResult<ProductTableDto>>
{
    public string? NumberId { get; set; }
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? PackageQuantity { get; set; }
    public string? PackageUnit { get; set; }
    public string? Status { get; set; }
}

public sealed class DashboardProductsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<DashboardProductsQuery, PagedResult<ProductTableDto>>
{
    public async Task<PagedResult<ProductTableDto>> Handle(
        DashboardProductsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await dbContext.Products
            .AsNoTracking()
            .ToPagedResultAsync<Product, Product, DashboardProductsQuery>(
                request,
                DashboardProductsQuery.Table,
                query => query,
                cancellationToken);

        return new PagedResult<ProductTableDto>(
            result.Items.Select(ProductTableDto.From).ToList(),
            result.FilteredCount,
            result.TotalCount);
    }
}
