using Domain.SeedWork;

namespace Domain.Entities;

public sealed class ProductBrand : BaseAuditableEntity, IAgregateRoot
{
    public const int MaxNameLength = 100;

    private ProductBrand()
    {
    }

    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;

    public static ProductBrand Create(string name)
    {
        var brand = new ProductBrand { Id = Guid.NewGuid() };
        brand.Rename(name);
        return brand;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("A Product Brand name is required.", nameof(name));
        }

        var normalized = string.Join(
            " ",
            name.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (normalized.Length > MaxNameLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(name),
                normalized.Length,
                $"A Product Brand name cannot exceed {MaxNameLength} characters.");
        }

        Name = normalized;
        NormalizedName = normalized.ToUpperInvariant();
    }
}
