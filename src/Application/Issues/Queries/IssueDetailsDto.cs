using Domain.Entities;

namespace Application.Issues.Queries;

/// <summary>Represents a worker assigned to an issue.</summary>
public sealed record IssueWorkerDto(string Id, string? UserName, string? Email)
{
    public static IssueWorkerDto From(ApplicationUser user) => new(user.Id, user.UserName, user.Email);
}

/// <summary>Represents the complete issue data returned by the API.</summary>
public sealed record IssueDetailsDto(
    Guid Id,
    int NumberId,
    Guid SiteId,
    string SiteName,
    string Title,
    string Description,
    string Status,
    DateOnly? StartDate,
    DateOnly? EndDate,
    DateTimeOffset Created,
    string? CreatedBy,
    DateTimeOffset LastModified,
    string? LastModifiedBy,
    IReadOnlyList<IssueWorkerDto> AssignedWorkers)
{
    public static IssueDetailsDto From(Issue issue) => new(
        issue.Id,
        issue.NumberId,
        issue.SiteId,
        issue.Site.Name.Value,
        issue.Title,
        issue.Description,
        issue.Status.ToString(),
        issue.StartDate,
        issue.EndDate,
        issue.Created,
        issue.CreatedBy,
        issue.LastModified,
        issue.LastModifiedBy,
        issue.AssignedWorkers
            .OrderBy(worker => worker.UserName ?? worker.Email ?? string.Empty)
            .Select(IssueWorkerDto.From)
            .ToList());
}
