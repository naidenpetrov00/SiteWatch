using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.Internal;
using MediatR;

namespace Application.Sites.Videos.Queries;

public sealed record GetVideoSnapshotQuery : IRequest<FileResponse>
{
    public Guid SnapshotId { get; init; }
}

public class GetVideoSnapshotHandler(IBlobService blobService) : IRequestHandler<GetVideoSnapshotQuery, FileResponse>
{
    public async Task<FileResponse> Handle(GetVideoSnapshotQuery request, CancellationToken cancellationToken) =>
        await blobService.DownloadImageAsync(request.SnapshotId, BlobContainerName.Images, cancellationToken);
}
