using Domain.Entities;
using Domain.SeedWork.Enums;

namespace Application.Products.Queries;

/// <summary>Represents an active product available for assignment lookup.</summary>
public sealed record ProductLookupDto
{
    public Guid Id { get; init; }
    public int NumberId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
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
            Category = product.Category,
            Brand = product.Brand,
            Model = product.Model,
            PackageQuantity = product.PackageQuantity,
            PackageUnit = product.PackageUnit?.ToCode()
        };
}
