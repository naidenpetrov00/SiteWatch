using Domain.SeedWork;

namespace Domain.Entities;

public sealed class Category : BaseAuditableEntity, IAgregateRoot
{
    public const int MaxNameLength = 200;

    private Category()
    {
    }

    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;

    public static Category Create(string name)
    {
        var category = new Category { Id = Guid.NewGuid() };
        category.Rename(name);
        return category;
    }

    public void Rename(string name)
    {
        Name = NormalizeName(name);
        NormalizedName = Name.ToUpperInvariant();
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("A category name is required.", nameof(name));
        }

        var normalized = string.Join(
            " ",
            name.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return normalized.Length <= MaxNameLength
            ? normalized
            : throw new ArgumentOutOfRangeException(
                nameof(name),
                normalized.Length,
                $"A category name cannot exceed {MaxNameLength} characters.");
    }
}
