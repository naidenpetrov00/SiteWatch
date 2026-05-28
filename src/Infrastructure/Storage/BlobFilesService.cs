using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.Internal;
using Application.Sites.Files.Commands;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Infrastructure.Storage;

internal sealed class BlobFilesService(BlobServiceClient blobServiceClient, IFilesService filesService)
    : IFilesBlobService
{
    public async Task<UploadedFileResult> UploadFileAsync(
        Stream stream,
        string contentType,
        BlobContainerName blobContainerName,
        CancellationToken cancellationToken = default)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(blobContainerName.ToString());

        await using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken);

        buffer.Position = 0;

        var fileId = Guid.NewGuid();
        var blobClient = containerClient.GetBlobClient(fileId.ToString());

        await blobClient.UploadAsync(
            buffer,
            new BlobHttpHeaders { ContentType = contentType },
            cancellationToken: cancellationToken);

        return new UploadedFileResult(fileId);
    }

    public async Task<FileResponse> DownloadFileAsync(
        Guid id,
        BlobContainerName blobContainerName,
        CancellationToken cancellationToken = default)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(blobContainerName.ToString());
        var blobClient = containerClient.GetBlobClient(id.ToString());

        var response = await blobClient.DownloadContentAsync(cancellationToken: cancellationToken);

        return new FileResponse(response.Value.Content.ToStream(), response.Value.Details.ContentType);
    }

    public async Task DeleteFileAsync(
        Guid id,
        BlobContainerName blobContainerName,
        CancellationToken cancellationToken = default)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(blobContainerName.ToString());
        var blobClient = containerClient.GetBlobClient(id.ToString());

        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        await filesService.DeleteFileIdFromSiteAsync(id, cancellationToken);
    }
}
