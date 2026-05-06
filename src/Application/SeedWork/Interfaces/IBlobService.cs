using Application.SeedWork.Models.Internal;
using Application.Sites.Images.Commands;

namespace Application.SeedWork.Interfaces;

public interface IBlobService
{
    Task<UploadedImageResult> UploadImageAsync(Stream stream, string contentType,
        BlobContainerName blobContainerName,
        CancellationToken cancellationToken = default);

    Task<FileResponse> DownloadImageAsync(Guid id, BlobContainerName blobContainerName,
        CancellationToken cancellationToken = default);

    Task DeleteImageAsync(Guid id, BlobContainerName blobContainerName, CancellationToken cancellationToken = default);
}