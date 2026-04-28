using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.Internal;

namespace Infrastructure.Storage;

internal sealed class BlobService : IBlobService
{
    public Task<Guid> UploadAsync(Stream stream, string contentType, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<FileResponse> DownloadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}