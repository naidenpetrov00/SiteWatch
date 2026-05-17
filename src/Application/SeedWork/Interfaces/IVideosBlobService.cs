using Application.SeedWork.Models.Internal;
using Application.Sites.Videos.Commands;

namespace Application.SeedWork.Interfaces;

public interface IVideosBlobService
{
    Task<UploadedVideoResult> UploadVideoAsync(
        Stream stream,
        string contentType,
        BlobContainerName blobContainerName,
        CancellationToken cancellationToken = default);

    Task<FileResponse> DownloadVideoAsync(
        Guid id,
        BlobContainerName blobContainerName,
        CancellationToken cancellationToken = default);

    Task DeleteVideoAsync(
        Guid id,
        BlobContainerName blobContainerName,
        CancellationToken cancellationToken = default);
}
