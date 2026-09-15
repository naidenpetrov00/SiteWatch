using Domain.SeedWork;

namespace Domain.Entities;

public abstract class ActivityCatalogNode : BaseAuditableEntity, IAgregateRoot
{
    public const int MaxNameLength = 200;

    private protected ActivityCatalogNode()
    {
    }

    private protected ActivityCatalogNode(string name, Guid? parentFolderId, int sortOrder)
    {
        Id = Guid.NewGuid();
        Rename(name);
        SetLocation(parentFolderId, sortOrder);
    }

    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;
    public Guid? ParentFolderId { get; private set; }
    public ActivityFolder? ParentFolder { get; private set; }
    public int SortOrder { get; private set; }

    public void Rename(string name)
    {
        var normalizedName = NormalizeName(name);
        Name = normalizedName;
        NormalizedName = NormalizeNameKey(normalizedName);
    }

    public static string NormalizeNameKey(string name) =>
        NormalizeName(name).ToUpperInvariant();

    public void MoveTo(ActivityFolder? parentFolder, int sortOrder)
    {
        if (parentFolder?.Id == Id)
        {
            throw new InvalidOperationException("A folder cannot be its own parent.");
        }

        ParentFolder = parentFolder;
        SetLocation(parentFolder?.Id, sortOrder);
    }

    public void SetSortOrder(int sortOrder)
    {
        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sortOrder),
                sortOrder,
                "Sort order cannot be negative.");
        }

        SortOrder = sortOrder;
    }

    private void SetLocation(Guid? parentFolderId, int sortOrder)
    {
        ParentFolderId = parentFolderId;
        SetSortOrder(sortOrder);
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("A catalog node name is required.", nameof(name));
        }

        var normalizedName = string.Join(
            " ",
            name.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

        if (normalizedName.Length > MaxNameLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(name),
                normalizedName.Length,
                $"A catalog node name cannot exceed {MaxNameLength} characters.");
        }

        return normalizedName;
    }
}
