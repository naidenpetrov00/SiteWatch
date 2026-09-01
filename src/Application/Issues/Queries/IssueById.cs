using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using MediatR;

namespace Application.Issues.Queries;

[Authorize(Roles = UserRoles.Administrator + "," + UserRoles.Client + "," + UserRoles.Worker)]
public sealed record IssueByIdQuery(Guid IssueId) : IRequest<IssueDetailsDto>;

public sealed class IssueByIdHandler(IIssueService issueService)
    : IRequestHandler<IssueByIdQuery, IssueDetailsDto>
{
    public Task<IssueDetailsDto> Handle(IssueByIdQuery request, CancellationToken cancellationToken) =>
        issueService.GetByIdAsync(request.IssueId, cancellationToken);
}
