using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Application.SeedWork.Queries;
using Application.SeedWork.Security;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Retailers.Queries;

/// <summary>Requests a filtered, sorted page of retailer storefronts.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed partial class DashboardRetailersQuery
    : TableQueryRequest, IRequest<PagedResult<RetailerTableDto>>
{
    public string? DisplayName { get; set; }
    public string? CompanyDisplayName { get; set; }
    public string? WebsiteHost { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class DashboardRetailersQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<DashboardRetailersQuery, PagedResult<RetailerTableDto>>
{
    public async Task<PagedResult<RetailerTableDto>> Handle(
        DashboardRetailersQuery request,
        CancellationToken cancellationToken)
    {
        var result = await dbContext.Retailers
            .AsNoTracking()
            .ToPagedResultAsync<Retailer, Retailer, DashboardRetailersQuery>(
                request,
                DashboardRetailersQuery.Table,
                query => query.Include(retailer => retailer.CompanyPerson),
                cancellationToken);

        return new PagedResult<RetailerTableDto>(
            result.Items.Select(RetailerTableDto.From).ToList(),
            result.FilteredCount,
            result.TotalCount);
    }
}

public sealed class DashboardRetailersQueryValidator
    : AbstractValidator<DashboardRetailersQuery>
{
    public DashboardRetailersQueryValidator()
    {
        RuleFor(query => query.PageIndex).GreaterThanOrEqualTo(0);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 1000);
        RuleFor(query => query.SortActive)
            .Must(sort => string.IsNullOrWhiteSpace(sort)
                || DashboardRetailersQuery.Table.Sorts.ContainsKey(sort.Trim()))
            .WithMessage("SortActive must be one of the retailer columns.");
        RuleFor(query => query.SortDirection)
            .Must(direction => string.IsNullOrWhiteSpace(direction)
                || direction.Equals("asc", StringComparison.OrdinalIgnoreCase)
                || direction.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortDirection must be asc or desc.");
    }
}
