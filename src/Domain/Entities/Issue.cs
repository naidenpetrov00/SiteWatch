using Ardalis.GuardClauses;
using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class Issue : BaseAuditableEntity, IHasNumberId
{
    private readonly HashSet<ApplicationUser> _assignedWorkers = [];
    private readonly HashSet<IssueAttachment> _attachments = [];

    private Issue()
    {
    }

    private Issue(Site site, string title, string description, IssueStatus status)
    {
        Id = Guid.NewGuid();
        Site = Guard.Against.Null(site);
        SiteId = site.Id;
        UpdateDetails(title, description, status, null, null);
    }

    public Guid SiteId { get; private set; }
    public Site Site { get; private set; } = null!;
    public int NumberId { get; private set; }
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public IssueStatus Status { get; private set; }
    public DateOnly? StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public IReadOnlyCollection<ApplicationUser> AssignedWorkers => _assignedWorkers;
    public IReadOnlyCollection<IssueAttachment> Attachments => _attachments;

    public static Issue Create(
        Site site,
        string title,
        string description,
        IssueStatus status = IssueStatus.Open) =>
        new(site, title, description, status);

    public void UpdateDetails(
        string title,
        string description,
        IssueStatus status,
        DateOnly? startDate,
        DateOnly? endDate)
    {
        Title = NormalizeRequiredText(title, nameof(title));
        Description = NormalizeRequiredText(description, nameof(description));
        Status = status;
        StartDate = startDate;
        EndDate = ValidateEndDate(startDate, endDate);
    }

    public void ChangeSite(Site site)
    {
        Site = Guard.Against.Null(site);
        SiteId = site.Id;
    }

    public void ReplaceAssignedWorkers(IEnumerable<ApplicationUser> workers)
    {
        var normalizedWorkers = Guard.Against.Null(workers).ToList();
        if (normalizedWorkers.Select(worker => worker.Id).Distinct(StringComparer.Ordinal).Count()
            != normalizedWorkers.Count)
        {
            throw new ArgumentException("A worker can only be assigned once.", nameof(workers));
        }

        _assignedWorkers.Clear();
        foreach (var worker in normalizedWorkers)
        {
            _assignedWorkers.Add(worker);
        }
    }

    private static DateOnly? ValidateEndDate(DateOnly? startDate, DateOnly? endDate)
    {
        if (startDate.HasValue && endDate.HasValue && endDate.Value < startDate.Value)
        {
            throw new ArgumentException("End date cannot be before the start date.", nameof(endDate));
        }

        return endDate;
    }

    private static string NormalizeRequiredText(string value, string parameterName) =>
        Guard.Against.NullOrWhiteSpace(value, parameterName).Trim();
}
