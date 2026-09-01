using Application.Issues.Attachments;

namespace Application.SeedWork.Interfaces;

public interface IIssueAttachmentService
{
    Task<IReadOnlyList<IssueAttachmentDto>> GetByIssueIdAsync(
        Guid issueId,
        CancellationToken cancellationToken);

    Task<IssueAttachmentDto> GetAsync(
        Guid issueId,
        Guid attachmentId,
        CancellationToken cancellationToken);

    Task<IssueAttachmentDto> AddAsync(
        Guid issueId,
        UploadedIssueAttachment file,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        Guid issueId,
        Guid attachmentId,
        CancellationToken cancellationToken);

    Task<IssueAttachmentFileResponse> OpenReadAsync(
        Guid issueId,
        Guid attachmentId,
        bool preview,
        CancellationToken cancellationToken);
}
