using Ardalis.GuardClauses;
using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class IssueAttachment : BaseAuditableEntity
{
    private IssueAttachment()
    {
    }

    public IssueAttachment(
        Guid id,
        Guid issueId,
        string fileName,
        string contentType,
        long sizeBytes,
        IssueAttachmentKind kind,
        Guid? previewBlobId = null,
        int? durationSeconds = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Attachment ID cannot be empty.", nameof(id));
        }

        if (issueId == Guid.Empty)
        {
            throw new ArgumentException("Issue ID cannot be empty.", nameof(issueId));
        }

        if (sizeBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sizeBytes), "Attachment size must be positive.");
        }

        Id = id;
        IssueId = issueId;
        FileName = Guard.Against.NullOrWhiteSpace(fileName).Trim();
        ContentType = Guard.Against.NullOrWhiteSpace(contentType).Trim();
        SizeBytes = sizeBytes;
        Kind = kind;
        PreviewBlobId = previewBlobId;
        DurationSeconds = durationSeconds;
    }

    public Guid IssueId { get; private set; }
    public Issue Issue { get; private set; } = null!;
    public string FileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long SizeBytes { get; private set; }
    public IssueAttachmentKind Kind { get; private set; }
    public Guid? PreviewBlobId { get; private set; }
    public int? DurationSeconds { get; private set; }
}
