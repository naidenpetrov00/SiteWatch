namespace Application.Issues.Commands;

/// <summary>Defines the editable fields of an issue.</summary>
public abstract record IssueUpsertDto
{
    public Guid SiteId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Status { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public List<string> AssignedWorkerIds { get; init; } = [];
}
