using Domain.Entities;
using Domain.SeedWork.Enums;

namespace Application.Products.Queries;

/// <summary>Represents the complete editable details of a product.</summary>
public sealed record ProductDetailsDto
{
    public Guid Id { get; init; }
    public int NumberId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Brand { get; init; }
    public string? Model { get; init; }
    public decimal? PackageQuantity { get; init; }
    public string? PackageUnit { get; init; }
    public Guid ProductTypeId { get; init; }
    public string ProductTypeName { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? PrimarySearchPhrase { get; init; }
    public string EffectivePrimarySearchPhrase { get; init; } = string.Empty;
    public IReadOnlyList<string> AlternativeSearchPhrases { get; init; } = [];
    public IReadOnlyList<string> RequiredKeywords { get; init; } = [];
    public IReadOnlyList<string> ExcludedKeywords { get; init; } = [];

    public static ProductDetailsDto From(Product product) =>
        new()
        {
            Id = product.Id,
            NumberId = product.NumberId,
            Title = product.Title,
            Description = product.Description,
            Brand = product.Brand,
            Model = product.Model,
            PackageQuantity = product.PackageQuantity,
            PackageUnit = product.PackageUnit?.ToCode(),
            ProductTypeId = product.ProductTypeId,
            ProductTypeName = product.ProductType.Name,
            CategoryId = product.ProductType.CategoryId,
            CategoryName = product.ProductType.Category.Name,
            Status = product.Status.ToString(),
            PrimarySearchPhrase = product.SearchConfiguration.PrimarySearchPhrase,
            EffectivePrimarySearchPhrase = product.EffectivePrimarySearchPhrase,
            AlternativeSearchPhrases = product.SearchConfiguration.AlternativeSearchPhrases,
            RequiredKeywords = product.SearchConfiguration.RequiredKeywords,
            ExcludedKeywords = product.SearchConfiguration.ExcludedKeywords
        };
}
