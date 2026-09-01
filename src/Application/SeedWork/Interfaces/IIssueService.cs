using Application.Issues.Commands;
using Application.Issues.Queries;
using Application.SeedWork.Models;

namespace Application.SeedWork.Interfaces;

public interface IIssueService
{
    Task<Guid> CreateAsync(CreateIssueCommand request, CancellationToken cancellationToken);
    Task UpdateAsync(UpdateIssueCommand request, CancellationToken cancellationToken);
    Task<IssueDetailsDto> GetByIdAsync(Guid issueId, CancellationToken cancellationToken);
    Task<IReadOnlyList<IssueDetailsDto>> GetBySiteAsync(Guid siteId, CancellationToken cancellationToken);
    Task<PagedResult<IssueDetailsDto>> GetDashboardIssuesAsync(
        DashboardIssuesQuery request,
        CancellationToken cancellationToken);
}
