namespace Domain.Entities;

public sealed class ActivityFolder : ActivityCatalogNode
{
    private readonly List<ActivityCatalogNode> _children = [];

    private ActivityFolder()
    {
    }

    private ActivityFolder(string name, Guid? parentFolderId, int sortOrder)
        : base(name, parentFolderId, sortOrder)
    {
    }

    public IReadOnlyCollection<ActivityCatalogNode> Children => _children;

    public static ActivityFolder Create(string name, Guid? parentFolderId, int sortOrder) =>
        new(name, parentFolderId, sortOrder);
}
