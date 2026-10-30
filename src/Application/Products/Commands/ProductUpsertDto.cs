using Domain.SeedWork.Enums;

namespace Application.Products.Commands;

/// <summary>Contains the editable fields shared by product create and update operations.</summary>
public abstract record ProductUpsertDto
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Brand { get; init; }
    public string? Model { get; init; }
    public decimal? PackageQuantity { get; init; }
    public string? PackageUnit { get; init; }
    public Guid ProductTypeId { get; init; }
    public string Status { get; init; } = ProductStatus.Active.ToString();
    public string? PrimarySearchPhrase { get; init; }
    public string[] AlternativeSearchPhrases { get; init; } = [];
    public string[] RequiredKeywords { get; init; } = [];
    public string[] ExcludedKeywords { get; init; } = [];
}
