using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.Internal;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Infrastructure.Storage;

internal sealed class LocalInvoiceFileStorage(BlobServiceClient blobServiceClient) : IInvoiceFileStorage
{
    public async Task<Guid> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        _ = fileName;

        var containerClient = blobServiceClient.GetBlobContainerClient(BlobContainerName.Invoices.ToString());

        await using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;

        var fileId = Guid.NewGuid();
        var blobClient = containerClient.GetBlobClient(fileId.ToString());

        await blobClient.UploadAsync(
            buffer,
            new BlobHttpHeaders { ContentType = contentType },
            cancellationToken: cancellationToken);

        return fileId;
    }

    public async Task<FileResponse> DownloadAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(BlobContainerName.Invoices.ToString());
        var blobClient = containerClient.GetBlobClient(fileId.ToString());

        var response = await blobClient.DownloadContentAsync(cancellationToken: cancellationToken);

        return new FileResponse(response.Value.Content.ToStream(), response.Value.Details.ContentType);
    }

    public async Task DeleteAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(BlobContainerName.Invoices.ToString());
        var blobClient = containerClient.GetBlobClient(fileId.ToString());

        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}
