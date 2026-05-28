using Application.SeedWork.Models.Internal;
using Application.Sites.Files.Commands;

namespace Application.SeedWork.Interfaces;

public interface IFilesBlobService
{
    Task<UploadedFileResult> UploadFileAsync(
        Stream stream,
        string contentType,
        BlobContainerName blobContainerName,
        CancellationToken cancellationToken = default);

    Task<FileResponse> DownloadFileAsync(
        Guid id,
        BlobContainerName blobContainerName,
        CancellationToken cancellationToken = default);

    Task DeleteFileAsync(
        Guid id,
        BlobContainerName blobContainerName,
        CancellationToken cancellationToken = default);
}
