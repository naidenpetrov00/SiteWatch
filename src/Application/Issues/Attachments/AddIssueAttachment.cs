using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using MediatR;

namespace Application.Issues.Attachments;

[Authorize(Roles = UserRoles.Administrator)]
public sealed record AddIssueAttachmentCommand(
    Guid IssueId,
    UploadedIssueAttachment File) : IRequest<IssueAttachmentDto>;

public sealed class AddIssueAttachmentHandler(IIssueAttachmentService attachmentService)
    : IRequestHandler<AddIssueAttachmentCommand, IssueAttachmentDto>
{
    public Task<IssueAttachmentDto> Handle(
        AddIssueAttachmentCommand request,
        CancellationToken cancellationToken) => attachmentService.AddAsync(
        request.IssueId,
        request.File,
        cancellationToken);
}
