using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class Activity : ActivityCatalogNode, IHasNumberId
{
    public const int MaxDescriptionLength = 2000;

    private readonly List<ActivityRequirementSection> _requirementSections = [];

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
    public IReadOnlyCollection<ActivityRequirementSection> RequirementSections =>
        _requirementSections;

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

    public ActivityRequirementSection AddRequirementSection(
        string? name,
        decimal basisQuantity,
        ActivityMeasurementUnit measurementUnit,
        int sortOrder)
    {
        EnsureRequirementsEditable();
        var section = ActivityRequirementSection.Create(
            this,
            name,
            basisQuantity,
            measurementUnit,
            sortOrder);
        _requirementSections.Add(section);
        return section;
    }

    public void RemoveRequirementSection(ActivityRequirementSection section)
    {
        EnsureRequirementsEditable();
        if (section.ActivityId != Id || !_requirementSections.Remove(section))
        {
            throw new InvalidOperationException(
                "The requirement section does not belong to this activity.");
        }
    }

    public void EnsureRequirementsEditable()
    {
        if (Status == ActivityStatus.Archived)
        {
            throw new InvalidOperationException(
                "Requirements cannot be changed while the activity is archived.");
        }
    }

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
