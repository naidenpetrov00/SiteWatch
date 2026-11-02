using Domain.Entities;
using Domain.SeedWork.Enums;

namespace Application.Products.Queries;

/// <summary>Represents an active product available for assignment lookup.</summary>
public sealed record ProductLookupDto
{
    public Guid Id { get; init; }
    public int NumberId { get; init; }
    public string Title { get; init; } = string.Empty;
    public Guid ProductTypeId { get; init; }
    public string ProductTypeName { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string? Brand { get; init; }
    public string? Model { get; init; }
    public decimal? PackageQuantity { get; init; }
    public string? PackageUnit { get; init; }

    public static ProductLookupDto From(Product product) =>
        new()
        {
            Id = product.Id,
            NumberId = product.NumberId,
            Title = product.Title,
            ProductTypeId = product.ProductTypeId,
            ProductTypeName = product.ProductType.Name,
            CategoryId = product.ProductType.CategoryId,
            CategoryName = product.ProductType.Category.Name,
            Brand = product.Brand,
            Model = product.Model,
            PackageQuantity = product.PackageQuantity,
            PackageUnit = product.PackageUnit?.ToCode()
        };
}
