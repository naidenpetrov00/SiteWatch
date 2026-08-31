using FluentValidation;
using Domain.SeedWork.Enums;

namespace Application.Issues.Commands;

public abstract class IssueUpsertValidator<TIssue> : AbstractValidator<TIssue>
    where TIssue : IssueUpsertDto
{
    protected IssueUpsertValidator()
    {
        RuleFor(issue => issue.SiteId).NotEmpty();
        RuleFor(issue => issue.Title).NotEmpty().MaximumLength(200);
        RuleFor(issue => issue.Description).NotEmpty().MaximumLength(4000);
        RuleFor(issue => issue.AssignedWorkerIds)
            .NotNull()
            .Must(workerIds => workerIds is not null
                && workerIds.Distinct(StringComparer.Ordinal).Count() == workerIds.Count)
            .WithMessage("A worker can only be assigned once.");
        RuleFor(issue => issue.Status)
            .Must(status => string.IsNullOrWhiteSpace(status)
                || (Enum.TryParse<IssueStatus>(status, true, out var parsed)
                    && Enum.IsDefined(parsed)))
            .WithMessage("Unsupported issue status.");
        RuleFor(issue => issue)
            .Must(issue => !issue.StartDate.HasValue
                || !issue.EndDate.HasValue
                || issue.EndDate.Value >= issue.StartDate.Value)
            .WithMessage("End date cannot be before the start date.");
    }
}
