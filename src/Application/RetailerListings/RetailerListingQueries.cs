using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Application.SeedWork.Queries;
using Application.SeedWork.Security;
using FluentValidation;
using MediatR;

namespace Application.RetailerListings;

public static class RetailerListingSorts
{
    public static readonly IReadOnlySet<string> All = new HashSet<string>(
        [
            "retailer",
            "productNumberId",
            "product",
            "latestPrice",
            "observedAt",
            "retailerProductCode",
            "isActive"
        ],
        StringComparer.OrdinalIgnoreCase);
}

/// <summary>Loads paged retailer listings for one Product.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed class ProductRetailerListingsQuery
    : TableQueryRequest, IRequest<PagedResult<RetailerListingDto>>
{
    public Guid ProductId { get; set; }
    public string? SearchTerm { get; set; }
    public bool IncludeInactive { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>Loads paged Product listings for one Retailer.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed class RetailerProductListingsQuery
    : TableQueryRequest, IRequest<PagedResult<RetailerListingDto>>
{
    public Guid RetailerId { get; set; }
    public string? SearchTerm { get; set; }
    public bool IncludeInactive { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>Loads one Product–Retailer listing by identifier.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record RetailerListingByIdQuery(Guid ListingId)
    : IRequest<RetailerListingDto>;

/// <summary>Loads newest-first price history for one Product–Retailer listing.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record RetailerPriceHistoryQuery : IRequest<RetailerPriceHistoryDto>
{
    public Guid ProductId { get; set; }
    public Guid RetailerId { get; set; }
    public int PageIndex { get; init; }
    public int PageSize { get; init; } = 25;
}

public sealed class ProductRetailerListingsValidator
    : AbstractValidator<ProductRetailerListingsQuery>
{
    public ProductRetailerListingsValidator()
    {
        RuleFor(query => query.ProductId).NotEmpty();
        RuleFor(query => query.SearchTerm).MaximumLength(200);
        RetailerListingQueryValidation.AddRules(this);
    }
}

public sealed class RetailerProductListingsValidator
    : AbstractValidator<RetailerProductListingsQuery>
{
    public RetailerProductListingsValidator()
    {
        RuleFor(query => query.RetailerId).NotEmpty();
        RuleFor(query => query.SearchTerm).MaximumLength(200);
        RetailerListingQueryValidation.AddRules(this);
    }
}

public sealed class RetailerListingByIdValidator
    : AbstractValidator<RetailerListingByIdQuery>
{
    public RetailerListingByIdValidator()
    {
        RuleFor(query => query.ListingId).NotEmpty();
    }
}

public sealed class RetailerPriceHistoryValidator
    : AbstractValidator<RetailerPriceHistoryQuery>
{
    public RetailerPriceHistoryValidator()
    {
        RuleFor(query => query.ProductId).NotEmpty();
        RuleFor(query => query.RetailerId).NotEmpty();
        RuleFor(query => query.PageIndex).GreaterThanOrEqualTo(0);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class ProductRetailerListingsHandler(IRetailerListingService service)
    : IRequestHandler<ProductRetailerListingsQuery, PagedResult<RetailerListingDto>>
{
    public Task<PagedResult<RetailerListingDto>> Handle(
        ProductRetailerListingsQuery request,
        CancellationToken cancellationToken) =>
        service.GetForProductAsync(request, cancellationToken);
}

public sealed class RetailerProductListingsHandler(IRetailerListingService service)
    : IRequestHandler<RetailerProductListingsQuery, PagedResult<RetailerListingDto>>
{
    public Task<PagedResult<RetailerListingDto>> Handle(
        RetailerProductListingsQuery request,
        CancellationToken cancellationToken) =>
        service.GetForRetailerAsync(request, cancellationToken);
}

public sealed class RetailerListingByIdHandler(IRetailerListingService service)
    : IRequestHandler<RetailerListingByIdQuery, RetailerListingDto>
{
    public Task<RetailerListingDto> Handle(
        RetailerListingByIdQuery request,
        CancellationToken cancellationToken) =>
        service.GetByIdAsync(request.ListingId, cancellationToken);
}

public sealed class RetailerPriceHistoryHandler(IRetailerListingService service)
    : IRequestHandler<RetailerPriceHistoryQuery, RetailerPriceHistoryDto>
{
    public Task<RetailerPriceHistoryDto> Handle(
        RetailerPriceHistoryQuery request,
        CancellationToken cancellationToken) =>
        service.GetHistoryAsync(
            request.ProductId,
            request.RetailerId,
            request.PageIndex,
            request.PageSize,
            cancellationToken);
}

internal static class RetailerListingQueryValidation
{
    internal static void AddRules<TQuery>(AbstractValidator<TQuery> validator)
        where TQuery : TableQueryRequest
    {
        validator.RuleFor(query => query.PageIndex).GreaterThanOrEqualTo(0);
        validator.RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        validator.RuleFor(query => query.SortActive)
            .Must(sort => string.IsNullOrWhiteSpace(sort)
                || RetailerListingSorts.All.Contains(sort.Trim()))
            .WithMessage("SortActive must be one of the retailer listing columns.");
        validator.RuleFor(query => query.SortDirection)
            .Must(direction => string.IsNullOrWhiteSpace(direction)
                || direction.Equals("asc", StringComparison.OrdinalIgnoreCase)
                || direction.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortDirection must be asc or desc.");
    }
}
