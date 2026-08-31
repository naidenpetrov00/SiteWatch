using Application.SeedWork.Queries;
using Domain.Entities;

namespace Application.Issues.Queries;

public sealed partial class DashboardIssuesQuery
{
    public static readonly TableQueryDefinition<Issue, DashboardIssuesQuery> Table = new(
        Filters:
        [
            TableFilterDescriptorExtensions.IntEquals<Issue, DashboardIssuesQuery>(
                "numberId", query => query.NumberId, issue => issue.NumberId),
            TableFilterDescriptorExtensions.GuidEquals<Issue, DashboardIssuesQuery>(
                "id", query => query.Id, issue => issue.Id),
            TableFilterDescriptor<Issue, DashboardIssuesQuery>.TextContains(
                "siteName", query => query.SiteName, issue => issue.Site.Name.Value),
            TableFilterDescriptor<Issue, DashboardIssuesQuery>.TextContains(
                "title", query => query.Title, issue => issue.Title),
            TableFilterDescriptor<Issue, DashboardIssuesQuery>.TextContains(
                "description", query => query.Description, issue => issue.Description),
            new TableFilterDescriptor<Issue, DashboardIssuesQuery>(
                "status", BuildStatusPredicate),
            TableFilterDescriptorExtensions.DateOnlySearch<Issue, DashboardIssuesQuery>(
                "startDate", query => query.StartDate, issue => issue.StartDate),
            TableFilterDescriptorExtensions.DateOnlySearch<Issue, DashboardIssuesQuery>(
                "endDate", query => query.EndDate, issue => issue.EndDate),
            TableFilterDescriptor<Issue, DashboardIssuesQuery>.DateTimeOffsetSearch(
                "created", query => query.Created, issue => (DateTimeOffset?)issue.Created),
            new TableFilterDescriptor<Issue, DashboardIssuesQuery>(
                "worker", BuildWorkerPredicate)
        ],
        Sorts: new Dictionary<string, TableSortDescriptor<Issue, DashboardIssuesQuery>>(
            StringComparer.OrdinalIgnoreCase)
        {
            ["numberId"] = TableSortDescriptor<Issue, DashboardIssuesQuery>.Create(
                "numberId", issue => issue.NumberId, issue => issue.Id),
            ["id"] = TableSortDescriptor<Issue, DashboardIssuesQuery>.Create(
                "id", issue => issue.Id),
            ["siteName"] = TableSortDescriptor<Issue, DashboardIssuesQuery>.Create(
                "siteName", issue => issue.Site.Name.Value, issue => issue.Id),
            ["title"] = TableSortDescriptor<Issue, DashboardIssuesQuery>.Create(
                "title", issue => issue.Title, issue => issue.Id),
            ["description"] = TableSortDescriptor<Issue, DashboardIssuesQuery>.Create(
                "description", issue => issue.Description, issue => issue.Id),
            ["status"] = TableSortDescriptor<Issue, DashboardIssuesQuery>.Create(
                "status", issue => issue.Status, issue => issue.Id),
            ["startDate"] = TableSortDescriptor<Issue, DashboardIssuesQuery>.Create(
                "startDate", issue => issue.StartDate ?? DateOnly.MinValue, issue => issue.Id),
            ["endDate"] = TableSortDescriptor<Issue, DashboardIssuesQuery>.Create(
                "endDate", issue => issue.EndDate ?? DateOnly.MinValue, issue => issue.Id),
            ["created"] = TableSortDescriptor<Issue, DashboardIssuesQuery>.Create(
                "created", issue => issue.Created, issue => issue.Id)
        },
        DefaultSort: query => query.OrderByDescending(issue => issue.Created).ThenBy(issue => issue.Id));

    private static System.Linq.Expressions.Expression<Func<Issue, bool>>? BuildWorkerPredicate(
        DashboardIssuesQuery query)
    {
        var value = query.Worker?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return issue => issue.AssignedWorkers.Any(worker =>
            (worker.UserName ?? string.Empty).ToLower().Contains(value)
            || (worker.Email ?? string.Empty).ToLower().Contains(value));
    }

    private static System.Linq.Expressions.Expression<Func<Issue, bool>>? BuildStatusPredicate(
        DashboardIssuesQuery query)
    {
        if (!Enum.TryParse<Domain.SeedWork.Enums.IssueStatus>(query.Status, true, out var status)
            || !Enum.IsDefined(status))
        {
            return null;
        }

        return issue => issue.Status == status;
    }
}
