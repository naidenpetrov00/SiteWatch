using Domain.Entities;
using Domain.SeedWork.Enums;

namespace Application.Products.Queries;

/// <summary>Represents a product row in the administrative table.</summary>
public sealed record ProductTableDto
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
    public string Status { get; init; } = string.Empty;

    public static ProductTableDto From(Product product) =>
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
            PackageUnit = product.PackageUnit?.ToCode(),
            Status = product.Status.ToString()
        };
}
