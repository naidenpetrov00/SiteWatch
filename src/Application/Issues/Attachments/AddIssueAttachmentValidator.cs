using Application.SeedWork.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Issues.Attachments;

public sealed class AddIssueAttachmentValidator : AbstractValidator<AddIssueAttachmentCommand>
{
    public AddIssueAttachmentValidator(IApplicationDbContext dbContext)
    {
        RuleFor(request => request.IssueId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MustAsync(async (issueId, cancellationToken) =>
                await dbContext.Issues.AsNoTracking().AnyAsync(
                    issue => issue.Id == issueId,
                    cancellationToken))
            .WithMessage("Issue does not exist.");

        RuleFor(request => request.File).NotNull();
        When(request => request.File is not null, () =>
        {
            RuleFor(request => request.File.FileName)
                .NotEmpty()
                .MaximumLength(IssueAttachmentValidation.MaxFileNameLength);
            RuleFor(request => request.File.ContentType)
                .NotEmpty()
                .MaximumLength(IssueAttachmentValidation.MaxContentTypeLength);
            RuleFor(request => request.File.SizeBytes).GreaterThan(0);
            RuleFor(request => request.File).Custom(ValidateFile);
        });
    }

    private static void ValidateFile(
        UploadedIssueAttachment? file,
        ValidationContext<AddIssueAttachmentCommand> context)
    {
        if (file is null)
        {
            return;
        }

        try
        {
            var kind = IssueAttachmentValidation.GetKind(file.ContentType);
            var maxSize = IssueAttachmentValidation.GetMaxFileSize(kind);
            if (file.SizeBytes > maxSize)
            {
                context.AddFailure(
                    nameof(file.SizeBytes),
                    $"The {kind.ToString().ToLowerInvariant()} cannot exceed {maxSize / 1024 / 1024} MB.");
            }
        }
        catch (ArgumentException exception)
        {
            context.AddFailure(nameof(file.ContentType), exception.Message);
        }
    }
}
