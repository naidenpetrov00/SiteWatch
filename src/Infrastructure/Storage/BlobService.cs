using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.Internal;
using Application.Sites.Images.Commands;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Infrastructure.SeedWork.Extension;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Infrastructure.Storage;

internal sealed class BlobService(BlobServiceClient blobServiceClient) : IBlobService
{
    public async Task<UploadedImageResult> UploadAsync(Stream stream, string contentType,
        BlobContainerName blobContainerName,
        CancellationToken cancellationToken = default)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(blobContainerName.ToString());

        await using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken);

        var originalFileId = Guid.NewGuid();
        var originalBlobClient = containerClient.GetBlobClient(originalFileId.ToString());

        buffer.Position = 0;
        await originalBlobClient.UploadAsync(buffer, new BlobHttpHeaders { ContentType = contentType },
            cancellationToken: cancellationToken);

        buffer.Position = 0;
        await using var thumbnailStream = await CreateThumbnailAsync(buffer, cancellationToken);
        var thumbnailFileId = Guid.NewGuid();
        var thumbnailBlobClient = containerClient.GetBlobClient(thumbnailFileId.ToString());

        await thumbnailBlobClient.UploadAsync(thumbnailStream, new BlobHttpHeaders { ContentType = contentType },
            cancellationToken: cancellationToken);

        return new UploadedImageResult(originalFileId, thumbnailFileId);
    }

    public async Task<FileResponse> DownloadAsync(Guid fileId, BlobContainerName blobContainerName,
        CancellationToken cancellationToken = default)
    {
        var containerClient =
            blobServiceClient.GetBlobContainerClient(blobContainerName.ToString());
        var blobClient = containerClient.GetBlobClient(fileId.ToString());

        var response = await blobClient.DownloadContentAsync(cancellationToken: cancellationToken);

        return new FileResponse(response.Value.Content.ToStream(), response.Value.Details.ContentType);
    }

    public async Task DeleteAsync(Guid fileId, BlobContainerName blobContainerName,
        CancellationToken cancellationToken = default)
    {
        var containerClient =
            blobServiceClient.GetBlobContainerClient(blobContainerName.ToString());
        var blobClient = containerClient.GetBlobClient(fileId.ToString());

        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    private static async Task<Stream> CreateThumbnailAsync(
        Stream originalStream,
        CancellationToken cancellationToken = default)
    {
        originalStream.Position = 0;

        using var image = await Image.LoadAsync(originalStream, cancellationToken);

        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(400, 400),
            Mode = ResizeMode.Max,
        }));

        var output = new MemoryStream();

        await image.SaveAsJpegAsync(output, new JpegEncoder
        {
            Quality = 75
        }, cancellationToken);

        output.Position = 0;

        return output;
    }
}