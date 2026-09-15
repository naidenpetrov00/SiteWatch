using Domain.Entities;
using Domain.SeedWork.Enums;

namespace Application.Products.Queries;

/// <summary>Represents a product row in the administrative table.</summary>
public sealed record ProductTableDto
{
    public Guid Id { get; init; }
    public int NumberId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
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
            Category = product.Category.ToCode(),
            Brand = product.Brand,
            Model = product.Model,
            PackageQuantity = product.PackageQuantity,
            PackageUnit = product.PackageUnit?.ToCode(),
            Status = product.Status.ToString()
        };
}
