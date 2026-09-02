using Application.SeedWork.Interfaces;
using Application.SeedWork.Exceptions;
using Application.SeedWork.Security;
using MediatR;

namespace Application.Issues.Attachments;

[Authorize(Roles = UserRoles.Administrator + "," + UserRoles.Client + "," + UserRoles.Worker)]
public sealed record IssueAttachmentsQuery(Guid IssueId)
    : IRequest<IReadOnlyList<IssueAttachmentDto>>;

public sealed class IssueAttachmentsHandler(IIssueAttachmentService attachmentService)
    : IRequestHandler<IssueAttachmentsQuery, IReadOnlyList<IssueAttachmentDto>>
{
    public Task<IReadOnlyList<IssueAttachmentDto>> Handle(
        IssueAttachmentsQuery request,
        CancellationToken cancellationToken) => attachmentService.GetByIssueIdAsync(
        request.IssueId,
        cancellationToken);
}

[Authorize(Roles = UserRoles.Administrator + "," + UserRoles.Client + "," + UserRoles.Worker)]
public sealed record IssueAttachmentQuery(Guid IssueId, Guid AttachmentId)
    : IRequest<IssueAttachmentDto>;

public sealed class IssueAttachmentHandler(IIssueAttachmentService attachmentService)
    : IRequestHandler<IssueAttachmentQuery, IssueAttachmentDto>
{
    public Task<IssueAttachmentDto> Handle(
        IssueAttachmentQuery request,
        CancellationToken cancellationToken) => attachmentService.GetAsync(
        request.IssueId,
        request.AttachmentId,
        cancellationToken);
}

public sealed record IssueAttachmentContentQuery(
    Guid IssueId,
    Guid AttachmentId,
    string UserId,
    bool Preview) : IRequest<IssueAttachmentFileResponse>;

public sealed class IssueAttachmentContentHandler(
    IIssueAttachmentService attachmentService,
    IIdentityService identityService)
    : IRequestHandler<IssueAttachmentContentQuery, IssueAttachmentFileResponse>
{
    public async Task<IssueAttachmentFileResponse> Handle(
        IssueAttachmentContentQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId)
            || !await identityService.IsInRoleAsync(request.UserId, UserRoles.Administrator)
                && !await identityService.IsInRoleAsync(request.UserId, UserRoles.Client)
                && !await identityService.IsInRoleAsync(request.UserId, UserRoles.Worker))
        {
            throw new ForbiddenAccessException();
        }

        return await attachmentService.OpenReadAsync(
            request.IssueId,
            request.AttachmentId,
            request.Preview,
            cancellationToken);
    }
}
