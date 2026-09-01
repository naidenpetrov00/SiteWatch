using Domain.Entities;
using Domain.SeedWork.Enums;

namespace Application.Issues.Attachments;

/// <summary>Represents attachment metadata returned by the API.</summary>
public sealed record IssueAttachmentDto(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Kind,
    bool HasPreview,
    int? DurationSeconds,
    DateTimeOffset Created)
{
    public static IssueAttachmentDto From(IssueAttachment attachment) => new(
        attachment.Id,
        attachment.FileName,
        attachment.ContentType,
        attachment.SizeBytes,
        attachment.Kind.ToString(),
        attachment.PreviewBlobId.HasValue,
        attachment.DurationSeconds,
        attachment.Created);
}

public sealed record UploadedIssueAttachment(
    Stream Stream,
    string FileName,
    string ContentType,
    long SizeBytes);

public sealed record IssueAttachmentFileResponse(
    Stream Stream,
    string FileName,
    string ContentType,
    long ContentLength,
    IssueAttachmentKind Kind);
