using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using FluentValidation;
using MediatR;

namespace Application.Issues.Commands;

[Authorize(Roles = UserRoles.Administrator + "," + UserRoles.Client + "," + UserRoles.Worker)]
/// <summary>Creates an issue for a site.</summary>
public sealed record CreateIssueCommand : IssueUpsertDto, IRequest<Guid>;

public sealed class CreateIssueHandler(IIssueService issueService)
    : IRequestHandler<CreateIssueCommand, Guid>
{
    public Task<Guid> Handle(CreateIssueCommand request, CancellationToken cancellationToken) =>
        issueService.CreateAsync(request, cancellationToken);
}

public sealed class CreateIssueValidator : IssueUpsertValidator<CreateIssueCommand>
{
}
