using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class Activity : ActivityCatalogNode, IHasNumberId
{
    public const int MaxDescriptionLength = 2000;

    private Activity()
    {
    }

    private Activity(
        string name,
        string? description,
        Guid? parentFolderId,
        int sortOrder)
        : base(name, parentFolderId, sortOrder)
    {
        UpdateDescription(description);
        Status = ActivityStatus.Active;
    }

    public int NumberId { get; private set; }
    public string? Description { get; private set; }
    public ActivityStatus Status { get; private set; }

    public static Activity Create(
        string name,
        string? description,
        Guid? parentFolderId,
        int sortOrder) =>
        new(name, description, parentFolderId, sortOrder);

    public void UpdateDetails(string name, string? description)
    {
        Rename(name);
        UpdateDescription(description);
    }

    public void Archive() => Status = ActivityStatus.Archived;

    public void Restore() => Status = ActivityStatus.Active;

    private void UpdateDescription(string? description)
    {
        var normalizedDescription = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

        if (normalizedDescription?.Length > MaxDescriptionLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(description),
                normalizedDescription.Length,
                $"An activity description cannot exceed {MaxDescriptionLength} characters.");
        }

        Description = normalizedDescription;
    }
}
