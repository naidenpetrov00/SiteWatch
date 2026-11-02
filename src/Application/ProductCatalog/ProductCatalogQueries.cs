using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Application.SeedWork.Security;
using FluentValidation;
using MediatR;

namespace Application.ProductCatalog;

/// <summary>Gets one Category.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record CategoryByIdQuery(Guid Id) : IRequest<CategoryDto>;

/// <summary>Gets a page of Categories.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record CategoriesQuery : IRequest<PagedResult<CategoryDto>>
{
    public string? SearchTerm { get; init; }
    public int PageIndex { get; init; }
    public int PageSize { get; init; } = 100;
}

/// <summary>Gets one Product Type.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record ProductTypeByIdQuery(Guid Id) : IRequest<ProductTypeDto>;

/// <summary>Gets a page of Product Types.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record ProductTypesQuery : IRequest<PagedResult<ProductTypeDto>>
{
    public Guid? CategoryId { get; init; }
    public string? SearchTerm { get; init; }
    public int PageIndex { get; init; }
    public int PageSize { get; init; } = 100;
}

public sealed class ProductCatalogQueryHandler(IProductCatalogService service)
    : IRequestHandler<CategoryByIdQuery, CategoryDto>,
      IRequestHandler<CategoriesQuery, PagedResult<CategoryDto>>,
      IRequestHandler<ProductTypeByIdQuery, ProductTypeDto>,
      IRequestHandler<ProductTypesQuery, PagedResult<ProductTypeDto>>
{
    public Task<CategoryDto> Handle(CategoryByIdQuery request, CancellationToken cancellationToken) =>
        service.GetCategoryAsync(request.Id, cancellationToken);

    public Task<PagedResult<CategoryDto>> Handle(
        CategoriesQuery request,
        CancellationToken cancellationToken) =>
        service.GetCategoriesAsync(request, cancellationToken);

    public Task<ProductTypeDto> Handle(
        ProductTypeByIdQuery request,
        CancellationToken cancellationToken) =>
        service.GetProductTypeAsync(request.Id, cancellationToken);

    public Task<PagedResult<ProductTypeDto>> Handle(
        ProductTypesQuery request,
        CancellationToken cancellationToken) =>
        service.GetProductTypesAsync(request, cancellationToken);
}

public sealed class CategoriesQueryValidator : AbstractValidator<CategoriesQuery>
{
    public CategoriesQueryValidator()
    {
        RuleFor(request => request.SearchTerm).MaximumLength(200);
        RuleFor(request => request.PageIndex).GreaterThanOrEqualTo(0);
        RuleFor(request => request.PageSize).InclusiveBetween(1, 1000);
    }
}

public sealed class CategoryByIdQueryValidator : AbstractValidator<CategoryByIdQuery>
{
    public CategoryByIdQueryValidator() => RuleFor(request => request.Id).NotEmpty();
}

public sealed class ProductTypesQueryValidator : AbstractValidator<ProductTypesQuery>
{
    public ProductTypesQueryValidator()
    {
        RuleFor(request => request.CategoryId)
            .NotEqual(Guid.Empty)
            .When(request => request.CategoryId.HasValue);
        RuleFor(request => request.SearchTerm).MaximumLength(200);
        RuleFor(request => request.PageIndex).GreaterThanOrEqualTo(0);
        RuleFor(request => request.PageSize).InclusiveBetween(1, 1000);
    }
}

public sealed class ProductTypeByIdQueryValidator : AbstractValidator<ProductTypeByIdQuery>
{
    public ProductTypeByIdQueryValidator() => RuleFor(request => request.Id).NotEmpty();
}
