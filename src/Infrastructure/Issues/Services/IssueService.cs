using Application.Issues.Commands;
using Application.Issues.Queries;
using Application.SeedWork.Exceptions;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Application.SeedWork.Queries;
using Application.SeedWork.Security;
using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.SeedWork.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Issues.Services;

public sealed class IssueService(
    ApplicationDbContext dbContext,
    IUser user,
    IIdentityService identityService) : IIssueService
{
    public async Task<Guid> CreateAsync(CreateIssueCommand request, CancellationToken cancellationToken)
    {
        var isAdministrator = await IsAdministratorAsync(cancellationToken);
        if (!isAdministrator)
        {
            EnsureClientRequestOnly(request);
        }

        var site = await GetSiteAsync(request.SiteId, cancellationToken);
        if (!isAdministrator)
        {
            await EnsureCurrentUserCanAccessSiteAsync(site.Id, cancellationToken);
        }

        var status = isAdministrator
            ? ParseStatus(request.Status, IssueStatus.Open)
            : IssueStatus.Open;
        var issue = Issue.Create(site, request.Title, request.Description, status);

        if (isAdministrator)
        {
            issue.UpdateDetails(
                request.Title,
                request.Description,
                status,
                request.StartDate,
                request.EndDate);
            issue.ReplaceAssignedWorkers(
                await GetWorkersAsync(request.AssignedWorkerIds, cancellationToken));
        }

        SetAuditValues(issue, isNew: true);

        dbContext.Issues.Add(issue);
        await dbContext.SaveChangesAsync(cancellationToken);

        return issue.Id;
    }

    public async Task UpdateAsync(UpdateIssueCommand request, CancellationToken cancellationToken)
    {
        var issue = await dbContext.Issues
            .Include(item => item.AssignedWorkers)
            .SingleOrDefaultAsync(item => item.Id == request.Id, cancellationToken);

        if (issue is null)
        {
            throw new NotFoundException(nameof(Issue), request.Id.ToString());
        }

        var site = await GetSiteAsync(request.SiteId, cancellationToken);
        issue.ChangeSite(site);
        issue.UpdateDetails(
            request.Title,
            request.Description,
            ParseStatus(request.Status, IssueStatus.Open),
            request.StartDate,
            request.EndDate);
        issue.ReplaceAssignedWorkers(
            await GetWorkersAsync(request.AssignedWorkerIds, cancellationToken));
        SetAuditValues(issue, isNew: false);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IssueDetailsDto> GetByIdAsync(Guid issueId, CancellationToken cancellationToken)
    {
        var issue = await dbContext.Issues
            .AsNoTracking()
            .Include(item => item.Site)
            .Include(item => item.AssignedWorkers)
            .SingleOrDefaultAsync(item => item.Id == issueId, cancellationToken);

        if (issue is null)
        {
            throw new NotFoundException(nameof(Issue), issueId.ToString());
        }

        if (!await IsAdministratorAsync(cancellationToken))
        {
            await EnsureCurrentUserCanAccessSiteAsync(issue.SiteId, cancellationToken);
        }

        return IssueDetailsDto.From(issue);
    }

    public async Task<IReadOnlyList<IssueDetailsDto>> GetBySiteAsync(
        Guid siteId,
        CancellationToken cancellationToken)
    {
        await GetSiteAsync(siteId, cancellationToken);
        if (!await IsAdministratorAsync(cancellationToken))
        {
            await EnsureCurrentUserCanAccessSiteAsync(siteId, cancellationToken);
        }

        var issues = await dbContext.Issues
            .AsNoTracking()
            .Include(issue => issue.Site)
            .Include(issue => issue.AssignedWorkers)
            .Where(issue => issue.SiteId == siteId)
            .OrderByDescending(issue => issue.Created)
            .ThenByDescending(issue => issue.NumberId)
            .ToListAsync(cancellationToken);

        return issues.Select(IssueDetailsDto.From).ToList();
    }

    public async Task<PagedResult<IssueDetailsDto>> GetDashboardIssuesAsync(
        DashboardIssuesQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<Issue> issues = dbContext.Issues
            .AsNoTracking()
            .Include(issue => issue.Site)
            .Include(issue => issue.AssignedWorkers);

        if (!await IsAdministratorAsync(cancellationToken))
        {
            var userId = GetCurrentUserId();
            issues = issues.Where(issue => issue.Site.Users.Any(siteUser => siteUser.Id == userId));
        }

        var result = await issues.ToPagedResultAsync<Issue, Issue, DashboardIssuesQuery>(
            request,
            DashboardIssuesQuery.Table,
            query => query,
            cancellationToken);

        return new PagedResult<IssueDetailsDto>(
            result.Items.Select(IssueDetailsDto.From).ToList(),
            result.FilteredCount,
            result.TotalCount);
    }

    private async Task<Site> GetSiteAsync(Guid siteId, CancellationToken cancellationToken)
    {
        var site = await dbContext.Sites
            .SingleOrDefaultAsync(item => item.Id == siteId, cancellationToken);

        return site ?? throw new NotFoundException(nameof(Site), siteId.ToString());
    }

    private async Task<List<ApplicationUser>> GetWorkersAsync(
        IReadOnlyCollection<string> workerIds,
        CancellationToken cancellationToken)
    {
        if (workerIds.Count == 0)
        {
            return [];
        }

        var normalizedIds = workerIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();
        if (normalizedIds.Count != workerIds.Count)
        {
            throw new ArgumentException("Worker IDs must be non-empty and unique.", nameof(workerIds));
        }

        var workers = await dbContext.Users
            .Where(candidate => normalizedIds.Contains(candidate.Id))
            .ToListAsync(cancellationToken);
        if (workers.Count != normalizedIds.Count)
        {
            throw new ArgumentException("One or more assigned workers do not exist.", nameof(workerIds));
        }

        foreach (var worker in workers)
        {
            if (!await identityService.IsInRoleAsync(worker.Id, UserRoles.Worker))
            {
                throw new ArgumentException("Assigned users must have the Worker role.", nameof(workerIds));
            }
        }

        return workers;
    }

    private async Task EnsureCurrentUserCanAccessSiteAsync(Guid siteId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var canAccess = await dbContext.Sites
            .AsNoTracking()
            .AnyAsync(
                site => site.Id == siteId && site.Users.Any(siteUser => siteUser.Id == userId),
                cancellationToken);
        if (!canAccess)
        {
            throw new ForbiddenAccessException();
        }
    }

    private async Task<bool> IsAdministratorAsync(CancellationToken cancellationToken) =>
        await identityService.IsInRoleAsync(GetCurrentUserId(), UserRoles.Administrator);

    private string GetCurrentUserId() =>
        user.Id ?? throw new UnauthorizedAccessException();

    private void SetAuditValues(Issue issue, bool isNew)
    {
        var now = DateTimeOffset.UtcNow;
        var userId = GetCurrentUserId();
        if (isNew)
        {
            issue.Created = now;
            issue.CreatedBy = userId;
        }

        issue.LastModified = now;
        issue.LastModifiedBy = userId;
    }

    private static IssueStatus ParseStatus(string? value, IssueStatus defaultStatus)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultStatus;
        }

        if (Enum.TryParse<IssueStatus>(value, true, out var status)
            && Enum.IsDefined(status))
        {
            return status;
        }

        throw new ArgumentException("Unsupported issue status.", nameof(value));
    }

    private static void EnsureClientRequestOnly(CreateIssueCommand request)
    {
        if (request.Status is not null
            || request.StartDate.HasValue
            || request.EndDate.HasValue
            || request.AssignedWorkerIds.Count > 0)
        {
            throw new ForbiddenAccessException();
        }
    }
}
