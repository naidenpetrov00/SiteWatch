using Application.SeedWork.Models.Internal;

namespace Application.SeedWork.Interfaces;

public interface IInvoiceFileStorage
{
    Task<Guid> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<FileResponse> DownloadAsync(
        Guid fileId,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid fileId,
        CancellationToken cancellationToken = default);
}
