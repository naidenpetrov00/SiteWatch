using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Domain.SeedWork.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Products.Queries;

/// <summary>Searches active products for a compact assignment lookup.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record ProductSearchQuery : IRequest<List<ProductLookupDto>>
{
    public string? SearchTerm { get; init; }
}

public sealed class ProductSearchQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<ProductSearchQuery, List<ProductLookupDto>>
{
    public async Task<List<ProductLookupDto>> Handle(
        ProductSearchQuery request,
        CancellationToken cancellationToken)
    {
        var normalizedSearchTerm = NormalizeSearchTerm(request.SearchTerm);
        if (normalizedSearchTerm.Length == 0)
        {
            return [];
        }

        var hasNumberId = int.TryParse(request.SearchTerm?.Trim(), out var numberId);
        var products = await dbContext.Products
            .AsNoTracking()
            .Where(product =>
                product.Status == ProductStatus.Active
                && (product.SearchIdentity.Contains(normalizedSearchTerm)
                    || hasNumberId && product.NumberId == numberId))
            .OrderBy(product => product.Title)
            .ThenBy(product => product.Id)
            .Take(20)
            .ToListAsync(cancellationToken);

        return products.Select(ProductLookupDto.From).ToList();
    }

    private static string NormalizeSearchTerm(string? value) =>
        string.Join(
            " ",
            (value ?? string.Empty)
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            .ToUpperInvariant();
}

public sealed class ProductSearchQueryValidator : AbstractValidator<ProductSearchQuery>
{
    public ProductSearchQueryValidator()
    {
        RuleFor(query => query.SearchTerm).MaximumLength(200);
    }
}
