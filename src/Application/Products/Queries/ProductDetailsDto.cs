using Domain.Entities;

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
    public string Category { get; init; } = string.Empty;
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
            PackageUnit = product.PackageUnit,
            Category = product.Category,
            Status = product.Status.ToString(),
            PrimarySearchPhrase = product.SearchConfiguration.PrimarySearchPhrase,
            EffectivePrimarySearchPhrase = product.EffectivePrimarySearchPhrase,
            AlternativeSearchPhrases = product.SearchConfiguration.AlternativeSearchPhrases,
            RequiredKeywords = product.SearchConfiguration.RequiredKeywords,
            ExcludedKeywords = product.SearchConfiguration.ExcludedKeywords
        };
}
