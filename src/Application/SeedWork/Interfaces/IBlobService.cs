using Application.SeedWork.Models.Internal;
using Application.Sites.Images.Commands;

namespace Application.SeedWork.Interfaces;

public interface IBlobService
{
    Task<UploadedImageResult> UploadAsync(Stream stream, string contentType,
        BlobContainerName blobContainerName,
        CancellationToken cancellationToken = default);

    Task<FileResponse> DownloadAsync(Guid id, BlobContainerName blobContainerName,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, BlobContainerName blobContainerName, CancellationToken cancellationToken = default);
}