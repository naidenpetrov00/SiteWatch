using Domain.SeedWork;

namespace Domain.Entities;

public sealed class ProductType : BaseAuditableEntity, IAgregateRoot
{
    public const int MaxNameLength = 200;
    public const int MaxDescriptionLength = 2000;

    private ProductType()
    {
    }

    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public static ProductType Create(
        Category category,
        string name,
        string? description = null)
    {
        ArgumentNullException.ThrowIfNull(category);
        var productType = new ProductType
        {
            Id = Guid.NewGuid(),
            Category = category,
            CategoryId = category.Id
        };
        productType.Update(category, name, description);
        return productType;
    }

    public void Update(Category category, string name, string? description = null)
    {
        ArgumentNullException.ThrowIfNull(category);
        Category = category;
        CategoryId = category.Id;
        Rename(name);
        Description = NormalizeDescription(description);
    }

    private void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("A product type name is required.", nameof(name));
        }

        var normalized = string.Join(
            " ",
            name.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (normalized.Length > MaxNameLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(name),
                normalized.Length,
                $"A product type name cannot exceed {MaxNameLength} characters.");
        }

        Name = normalized;
        NormalizedName = normalized.ToUpperInvariant();
    }

    private static string? NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        var normalized = string.Join(
            " ",
            description.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (normalized.Length > MaxDescriptionLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(description),
                normalized.Length,
                $"A product type description cannot exceed {MaxDescriptionLength} characters.");
        }

        return normalized;
    }
}
