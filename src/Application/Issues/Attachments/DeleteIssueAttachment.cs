using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using MediatR;

namespace Application.Issues.Attachments;

[Authorize(Roles = UserRoles.Administrator)]
public sealed record DeleteIssueAttachmentCommand(Guid IssueId, Guid AttachmentId) : IRequest;

public sealed class DeleteIssueAttachmentHandler(IIssueAttachmentService attachmentService)
    : IRequestHandler<DeleteIssueAttachmentCommand>
{
    public Task Handle(
        DeleteIssueAttachmentCommand request,
        CancellationToken cancellationToken) => attachmentService.DeleteAsync(
        request.IssueId,
        request.AttachmentId,
        cancellationToken);
}
