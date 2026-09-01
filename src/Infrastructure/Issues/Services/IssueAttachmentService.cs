using Application.Issues.Attachments;
using Application.SeedWork.Exceptions;
using Application.SeedWork.Interfaces;
using Ardalis.GuardClauses;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Issues.Storage;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Issues.Services;

internal sealed class IssueAttachmentService(
    ApplicationDbContext dbContext,
    IssueAttachmentBlobStorage blobStorage,
    IUser user) : IIssueAttachmentService
{
    public async Task<IReadOnlyList<IssueAttachmentDto>> GetByIssueIdAsync(
        Guid issueId,
        CancellationToken cancellationToken)
    {
        await EnsureIssueExistsAsync(issueId, cancellationToken);

        return await dbContext.IssueAttachments
            .AsNoTracking()
            .Where(attachment => attachment.IssueId == issueId)
            .OrderByDescending(attachment => attachment.Created)
            .Select(attachment => new IssueAttachmentDto(
                attachment.Id,
                attachment.FileName,
                attachment.ContentType,
                attachment.SizeBytes,
                attachment.Kind.ToString(),
                attachment.PreviewBlobId.HasValue,
                attachment.DurationSeconds,
                attachment.Created))
            .ToListAsync(cancellationToken);
    }

    public async Task<IssueAttachmentDto> GetAsync(
        Guid issueId,
        Guid attachmentId,
        CancellationToken cancellationToken)
    {
        var attachment = await GetEntityAsync(issueId, attachmentId, cancellationToken);
        return IssueAttachmentDto.From(attachment);
    }

    public async Task<IssueAttachmentDto> AddAsync(
        Guid issueId,
        UploadedIssueAttachment file,
        CancellationToken cancellationToken)
    {
        await EnsureIssueExistsAsync(issueId, cancellationToken);

        var contentType = IssueAttachmentValidation.NormalizeContentType(file.ContentType);
        var kind = IssueAttachmentValidation.GetKind(contentType);
        var fileName = Path.GetFileName(file.FileName.Replace('\\', '/'));
        var userId = GetCurrentUserId();
        var stored = await blobStorage.UploadAsync(
            file.Stream,
            contentType,
            kind,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var attachment = new IssueAttachment(
            stored.AttachmentId,
            issueId,
            fileName,
            contentType,
            file.SizeBytes,
            kind,
            stored.PreviewBlobId,
            stored.DurationSeconds)
        {
            Created = now,
            CreatedBy = userId,
            LastModified = now,
            LastModifiedBy = userId,
        };

        try
        {
            dbContext.IssueAttachments.Add(attachment);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await blobStorage.DeleteIfExistsAsync(attachment, CancellationToken.None);
            throw;
        }

        return IssueAttachmentDto.From(attachment);
    }

    public async Task DeleteAsync(
        Guid issueId,
        Guid attachmentId,
        CancellationToken cancellationToken)
    {
        var attachment = await GetEntityAsync(issueId, attachmentId, cancellationToken);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        dbContext.IssueAttachments.Remove(attachment);
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await blobStorage.DeleteIfExistsAsync(attachment, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<IssueAttachmentFileResponse> OpenReadAsync(
        Guid issueId,
        Guid attachmentId,
        bool preview,
        CancellationToken cancellationToken)
    {
        var attachment = await GetEntityAsync(issueId, attachmentId, cancellationToken);
        return await blobStorage.OpenReadAsync(
            attachment,
            preview,
            cancellationToken);
    }

    private async Task EnsureIssueExistsAsync(Guid issueId, CancellationToken cancellationToken)
    {
        if (!await dbContext.Issues.AsNoTracking().AnyAsync(
                issue => issue.Id == issueId,
                cancellationToken))
        {
            throw new NotFoundException(nameof(Issue), issueId.ToString());
        }
    }

    private async Task<IssueAttachment> GetEntityAsync(
        Guid issueId,
        Guid attachmentId,
        CancellationToken cancellationToken) => await dbContext.IssueAttachments
        .SingleOrDefaultAsync(
            attachment => attachment.Id == attachmentId && attachment.IssueId == issueId,
            cancellationToken)
        ?? throw new NotFoundException(nameof(IssueAttachment), attachmentId.ToString());

    private string GetCurrentUserId() =>
        user.Id ?? throw new UnauthorizedAccessException();
}
