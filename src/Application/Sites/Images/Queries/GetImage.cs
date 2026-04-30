using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.Internal;
using MediatR;

namespace Application.Sites.Images.Queries;

public sealed record GetImageQuery : IRequest<FileResponse>
{
    public Guid FileId { get; init; }
}

public class GetImageHandler(IBlobService blobService) : IRequestHandler<GetImageQuery, FileResponse>
{
    public async Task<FileResponse> Handle(GetImageQuery request, CancellationToken cancellationToken) =>
        await blobService.DownloadAsync(request.FileId, BlobContainerName.Images,cancellationToken);
}