using Domain.SeedWork;

namespace Domain.Entities;

public sealed class ProductType : BaseAuditableEntity, IAgregateRoot
{
    public const int MaxNameLength = 200;

    private ProductType()
    {
    }

    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;

    public static ProductType Create(Category category, string name)
    {
        ArgumentNullException.ThrowIfNull(category);
        var productType = new ProductType
        {
            Id = Guid.NewGuid(),
            Category = category,
            CategoryId = category.Id
        };
        productType.Rename(name);
        return productType;
    }

    public void Update(Category category, string name)
    {
        ArgumentNullException.ThrowIfNull(category);
        Category = category;
        CategoryId = category.Id;
        Rename(name);
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
}
