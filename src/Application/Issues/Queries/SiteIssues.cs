using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using MediatR;

namespace Application.Issues.Queries;

[Authorize(Roles = UserRoles.Administrator + "," + UserRoles.Client + "," + UserRoles.Worker)]
/// <summary>Gets the issues for a site the current user can access.</summary>
public sealed record SiteIssuesQuery(Guid SiteId) : IRequest<IReadOnlyList<IssueDetailsDto>>;

public sealed class SiteIssuesHandler(IIssueService issueService)
    : IRequestHandler<SiteIssuesQuery, IReadOnlyList<IssueDetailsDto>>
{
    public Task<IReadOnlyList<IssueDetailsDto>> Handle(
        SiteIssuesQuery request,
        CancellationToken cancellationToken) =>
        issueService.GetBySiteAsync(request.SiteId, cancellationToken);
}
