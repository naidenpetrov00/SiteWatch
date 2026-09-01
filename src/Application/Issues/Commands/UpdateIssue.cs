using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using FluentValidation;
using MediatR;

namespace Application.Issues.Commands;

[Authorize(Roles = UserRoles.Administrator)]
/// <summary>Updates an existing issue.</summary>
public sealed record UpdateIssueCommand : IssueUpsertDto, IRequest
{
    public Guid Id { get; set; }
}

public sealed class UpdateIssueHandler(IIssueService issueService)
    : IRequestHandler<UpdateIssueCommand>
{
    public Task Handle(UpdateIssueCommand request, CancellationToken cancellationToken) =>
        issueService.UpdateAsync(request, cancellationToken);
}

public sealed class UpdateIssueValidator : IssueUpsertValidator<UpdateIssueCommand>
{
    public UpdateIssueValidator()
    {
        RuleFor(issue => issue.Id).NotEmpty();
        RuleFor(issue => issue.Status).NotEmpty();
    }
}
