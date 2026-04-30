using Application.SeedWork.Models.Internal;

namespace Application.SeedWork.Interfaces;

public interface IBlobService
{
    Task<Guid> UploadAsync(Stream stream, string contentType, BlobContainerName blobContainerName,
        CancellationToken cancellationToken = default);

    Task<FileResponse> DownloadAsync(Guid id, BlobContainerName blobContainerName,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, BlobContainerName blobContainerName, CancellationToken cancellationToken = default);
}