using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.Internal;
using Application.Sites.Videos.Commands;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Infrastructure.Storage;

internal sealed class BlobVideosService(
    BlobServiceClient blobServiceClient,
    IVideosService videosService,
    IVideoFileInspector videoFileInspector)
    : IVideosBlobService
{
    public async Task<UploadedVideoResult> UploadVideoAsync(
        Stream stream,
        string contentType,
        BlobContainerName blobContainerName,
        CancellationToken cancellationToken = default)
    {
        var inputPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.mp4");

        try
        {
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            await using (var inputFile = File.Create(inputPath))
            {
                await stream.CopyToAsync(inputFile, cancellationToken);
            }

            var inspection = await videoFileInspector.InspectAsync(
                inputPath,
                contentType,
                cancellationToken);

            await using var snapshotSource = File.OpenRead(inputPath);
            await using var snapshotStream = await videosService.CreateSnapshotAsync(
                snapshotSource,
                cancellationToken);

            var containerClient = blobServiceClient.GetBlobContainerClient(blobContainerName.ToString());
            var videoFileId = Guid.NewGuid();
            var videoBlobClient = containerClient.GetBlobClient(videoFileId.ToString());

            await using (var uploadStream = File.OpenRead(inputPath))
            {
                await videoBlobClient.UploadAsync(
                    uploadStream,
                    new BlobHttpHeaders { ContentType = contentType },
                    cancellationToken: cancellationToken);
            }

            var snapshotFileId = Guid.NewGuid();
            var snapshotContainerClient = blobServiceClient.GetBlobContainerClient(BlobContainerName.Images.ToString());
            var snapshotBlobClient = snapshotContainerClient.GetBlobClient(snapshotFileId.ToString());

            try
            {
                await snapshotBlobClient.UploadAsync(
                    snapshotStream,
                    new BlobHttpHeaders { ContentType = "image/jpeg" },
                    cancellationToken: cancellationToken);
            }
            catch
            {
                await videoBlobClient.DeleteIfExistsAsync(cancellationToken: CancellationToken.None);
                throw;
            }

            return new UploadedVideoResult(
                videoFileId,
                snapshotFileId,
                inspection.DurationSeconds);
        }
        finally
        {
            if (File.Exists(inputPath))
            {
                File.Delete(inputPath);
            }
        }
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
