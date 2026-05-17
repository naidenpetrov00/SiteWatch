using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.Internal;
using MediatR;

namespace Application.Sites.Videos.Queries;

public sealed record GetVideoQuery : IRequest<FileResponse>
{
    public Guid FileId { get; init; }
}

public class GetVideoHandler(IVideosBlobService blobService) : IRequestHandler<GetVideoQuery, FileResponse>
{
    public async Task<FileResponse> Handle(GetVideoQuery request, CancellationToken cancellationToken) =>
        await blobService.DownloadVideoAsync(request.FileId, BlobContainerName.Videos, cancellationToken);
}
