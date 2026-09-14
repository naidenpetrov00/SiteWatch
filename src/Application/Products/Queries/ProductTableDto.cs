using Domain.Entities;

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
    public string? ExternalIdentifierType { get; init; }
    public string? ExternalIdentifier { get; init; }
    public decimal? PackageQuantity { get; init; }
    public string? PackageUnit { get; init; }
    public string Status { get; init; } = string.Empty;

    public static ProductTableDto From(Product product) =>
        new()
        {
            Id = product.Id,
            NumberId = product.NumberId,
            Title = product.Title,
            Category = product.Category,
            Brand = product.Brand,
            Model = product.Model,
            ExternalIdentifierType = product.ExternalIdentifierType,
            ExternalIdentifier = product.ExternalIdentifier,
            PackageQuantity = product.PackageQuantity,
            PackageUnit = product.PackageUnit,
            Status = product.Status.ToString()
        };
}
