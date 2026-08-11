using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.Internal;
using Application.Sites.Images.Commands;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Infrastructure.Storage;

internal sealed class BlobImagesService(BlobServiceClient blobServiceClient, IImagesService imagesService) : IBlobService
{
    public async Task<UploadedImageResult> UploadImageAsync(Stream stream, string contentType,
        BlobContainerName blobContainerName,
        CancellationToken cancellationToken = default)
    {
        await using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken);

        buffer.Position = 0;
        await using var thumbnailStream = await imagesService.CreateThumbnailAsync(
            buffer,
            contentType,
            cancellationToken);

        var containerClient = blobServiceClient.GetBlobContainerClient(blobContainerName.ToString());
        var originalFileId = Guid.NewGuid();
        var originalBlobClient = containerClient.GetBlobClient(originalFileId.ToString());

        buffer.Position = 0;
        await originalBlobClient.UploadAsync(buffer, new BlobHttpHeaders { ContentType = contentType },
            cancellationToken: cancellationToken);

        var thumbnailFileId = Guid.NewGuid();
        var thumbnailBlobClient = containerClient.GetBlobClient(thumbnailFileId.ToString());

        try
        {
            await thumbnailBlobClient.UploadAsync(
                thumbnailStream,
                new BlobHttpHeaders { ContentType = "image/jpeg" },
                cancellationToken: cancellationToken);
        }
        catch
        {
            await originalBlobClient.DeleteIfExistsAsync(cancellationToken: CancellationToken.None);
            throw;
        }

        return new UploadedImageResult(originalFileId, thumbnailFileId);
    }

    public async Task<FileResponse> DownloadImageAsync(Guid fileId, BlobContainerName blobContainerName,
        CancellationToken cancellationToken = default)
    {
        var containerClient =
            blobServiceClient.GetBlobContainerClient(blobContainerName.ToString());
        var blobClient = containerClient.GetBlobClient(fileId.ToString());

        var response = await blobClient.DownloadContentAsync(cancellationToken: cancellationToken);

        return new FileResponse(response.Value.Content.ToStream(), response.Value.Details.ContentType);
    }

    public async Task DeleteImageAsync(Guid fileId, BlobContainerName blobContainerName,
        CancellationToken cancellationToken = default)
    {
        var containerClient =
            blobServiceClient.GetBlobContainerClient(blobContainerName.ToString());
        var blobClient = containerClient.GetBlobClient(fileId.ToString());

        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        await imagesService.DeleteImageIdFromSiteAsync(fileId, cancellationToken);
    }
}
