using Application.Issues.Attachments;
using Application.SeedWork.Exceptions;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.Internal;
using Ardalis.GuardClauses;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Domain.Entities;
using Domain.SeedWork.Enums;

namespace Infrastructure.Issues.Storage;

internal sealed class IssueAttachmentBlobStorage(
    BlobServiceClient blobServiceClient,
    IBlobService imagesBlobService,
    IVideosBlobService videosBlobService,
    IFilesBlobService filesBlobService,
    ILogger<IssueAttachmentBlobStorage> logger)
{
    public Task<StoredIssueAttachment> UploadAsync(
        Stream stream,
        string contentType,
        IssueAttachmentKind kind,
        CancellationToken cancellationToken) => kind switch
    {
        IssueAttachmentKind.Image => UploadImageAsync(stream, contentType, cancellationToken),
        IssueAttachmentKind.Video => UploadVideoAsync(stream, contentType, cancellationToken),
        _ => UploadFileAsync(stream, contentType, cancellationToken),
    };

    public async Task<IssueAttachmentFileResponse> OpenReadAsync(
        IssueAttachment attachment,
        bool preview,
        CancellationToken cancellationToken)
    {
        if (preview && attachment.PreviewBlobId is null)
        {
            throw new NotFoundException(
                "Issue attachment preview",
                attachment.Id.ToString());
        }

        var blobId = preview ? attachment.PreviewBlobId!.Value : attachment.Id;
        var containerName = preview
            ? BlobContainerName.Images
            : GetContainerName(attachment.Kind);

        try
        {
            var stream = await GetBlobClient(containerName, blobId).OpenReadAsync(
                new BlobOpenReadOptions(allowModifications: false),
                cancellationToken);
            var fileName = preview
                ? $"{Path.GetFileNameWithoutExtension(attachment.FileName)}-preview.jpg"
                : attachment.FileName;

            return new IssueAttachmentFileResponse(
                stream,
                fileName,
                preview ? "image/jpeg" : attachment.ContentType,
                stream.Length,
                attachment.Kind);
        }
        catch (RequestFailedException exception) when (exception.Status == 404)
        {
            logger.LogWarning(
                "Issue attachment content was not found in storage for attachment {AttachmentId} on issue {IssueId}. Preview {Preview}.",
                attachment.Id,
                attachment.IssueId,
                preview);
            throw new NotFoundException(
                "Issue attachment content",
                attachment.Id.ToString());
        }
        catch (RequestFailedException exception)
        {
            logger.LogError(
                exception,
                "Failed to access issue attachment content in storage for attachment {AttachmentId} on issue {IssueId}. Preview {Preview}, storage status {StorageStatus}.",
                attachment.Id,
                attachment.IssueId,
                preview,
                exception.Status);
            throw;
        }
    }

    public async Task DeleteIfExistsAsync(
        IssueAttachment attachment,
        CancellationToken cancellationToken)
    {
        if (attachment.PreviewBlobId.HasValue)
        {
            await GetBlobClient(BlobContainerName.Images, attachment.PreviewBlobId.Value)
                .DeleteIfExistsAsync(cancellationToken: cancellationToken);
        }

        await GetBlobClient(GetContainerName(attachment.Kind), attachment.Id)
            .DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    private async Task<StoredIssueAttachment> UploadImageAsync(
        Stream stream,
        string contentType,
        CancellationToken cancellationToken)
    {
        var result = await imagesBlobService.UploadImageAsync(
            stream,
            contentType,
            BlobContainerName.Images,
            cancellationToken);
        return new StoredIssueAttachment(
            result.OriginalFileId,
            result.ThumbnailFileId,
            null);
    }

    private async Task<StoredIssueAttachment> UploadVideoAsync(
        Stream stream,
        string contentType,
        CancellationToken cancellationToken)
    {
        var result = await videosBlobService.UploadVideoAsync(
            stream,
            contentType,
            BlobContainerName.Videos,
            cancellationToken);
        return new StoredIssueAttachment(
            result.VideoFileId,
            result.SnapshotFileId,
            result.DurationSeconds);
    }

    private async Task<StoredIssueAttachment> UploadFileAsync(
        Stream stream,
        string contentType,
        CancellationToken cancellationToken)
    {
        var result = await filesBlobService.UploadFileAsync(
            stream,
            contentType,
            BlobContainerName.Files,
            cancellationToken);
        return new StoredIssueAttachment(result.FileId, null, null);
    }

    private BlobClient GetBlobClient(BlobContainerName containerName, Guid blobId) =>
        blobServiceClient
            .GetBlobContainerClient(containerName.ToString())
            .GetBlobClient(blobId.ToString());

    private static BlobContainerName GetContainerName(IssueAttachmentKind kind) => kind switch
    {
        IssueAttachmentKind.Image => BlobContainerName.Images,
        IssueAttachmentKind.Video => BlobContainerName.Videos,
        _ => BlobContainerName.Files,
    };
}

internal sealed record StoredIssueAttachment(
    Guid AttachmentId,
    Guid? PreviewBlobId,
    int? DurationSeconds);
