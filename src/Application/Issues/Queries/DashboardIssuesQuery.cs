using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Application.SeedWork.Queries;
using Application.SeedWork.Security;
using MediatR;

namespace Application.Issues.Queries;

[Authorize(Roles = UserRoles.Administrator + "," + UserRoles.Client + "," + UserRoles.Worker)]
/// <summary>Defines filtering, sorting, and paging for the dashboard issue table.</summary>
public sealed partial class DashboardIssuesQuery
    : TableQueryRequest,
        IRequest<PagedResult<IssueDetailsDto>>
{
    public string? NumberId { get; set; }
    public string? Id { get; set; }
    public string? SiteName { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? Created { get; set; }
    public string? Worker { get; set; }
}

public sealed class DashboardIssuesHandler(IIssueService issueService)
    : IRequestHandler<DashboardIssuesQuery, PagedResult<IssueDetailsDto>>
{
    public Task<PagedResult<IssueDetailsDto>> Handle(
        DashboardIssuesQuery request,
        CancellationToken cancellationToken) =>
        issueService.GetDashboardIssuesAsync(request, cancellationToken);
}
