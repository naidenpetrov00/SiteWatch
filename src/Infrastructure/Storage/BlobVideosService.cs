using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.Internal;
using Application.Sites.Videos.Commands;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Infrastructure.Storage;

internal sealed class BlobVideosService(BlobServiceClient blobServiceClient, IVideosService videosService)
    : IVideosBlobService
{
    public async Task<UploadedVideoResult> UploadVideoAsync(
        Stream stream,
        string contentType,
        BlobContainerName blobContainerName,
        CancellationToken cancellationToken = default)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(blobContainerName.ToString());

        await using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken);

        var videoFileId = Guid.NewGuid();
        var videoBlobClient = containerClient.GetBlobClient(videoFileId.ToString());

        buffer.Position = 0;
        await videoBlobClient.UploadAsync(
            buffer,
            new BlobHttpHeaders { ContentType = contentType },
            cancellationToken: cancellationToken);

        buffer.Position = 0;
        await using var snapshotStream = await videosService.CreateSnapshotAsync(buffer, cancellationToken);
        var snapshotFileId = Guid.NewGuid();
        var snapshotContainerClient = blobServiceClient.GetBlobContainerClient(BlobContainerName.Images.ToString());
        var snapshotBlobClient = snapshotContainerClient.GetBlobClient(snapshotFileId.ToString());

        await snapshotBlobClient.UploadAsync(
            snapshotStream,
            new BlobHttpHeaders { ContentType = "image/jpeg" },
            cancellationToken: cancellationToken);

        return new UploadedVideoResult(videoFileId, snapshotFileId);
    }

    public async Task<FileResponse> DownloadVideoAsync(
        Guid fileId,
        BlobContainerName blobContainerName,
        CancellationToken cancellationToken = default)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(blobContainerName.ToString());
        var blobClient = containerClient.GetBlobClient(fileId.ToString());

        var response = await blobClient.DownloadContentAsync(cancellationToken: cancellationToken);

        return new FileResponse(response.Value.Content.ToStream(), response.Value.Details.ContentType);
    }

    public async Task DeleteVideoAsync(
        Guid fileId,
        BlobContainerName blobContainerName,
        CancellationToken cancellationToken = default)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(blobContainerName.ToString());
        var blobClient = containerClient.GetBlobClient(fileId.ToString());

        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        var snapshotId = await videosService.DeleteVideoIdFromSiteAsync(fileId, cancellationToken);

        if (snapshotId is not null)
        {
            var snapshotContainerClient = blobServiceClient.GetBlobContainerClient(BlobContainerName.Images.ToString());
            var snapshotBlobClient = snapshotContainerClient.GetBlobClient(snapshotId.Value.ToString());
            await snapshotBlobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        }
    }
}
