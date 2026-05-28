using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.Internal;
using MediatR;

namespace Application.Sites.Files.Queries;

public sealed record GetFileQuery : IRequest<FileResponse>
{
    public Guid FileId { get; init; }
}

public class GetFileHandler(IFilesBlobService blobService) : IRequestHandler<GetFileQuery, FileResponse>
{
    public async Task<FileResponse> Handle(GetFileQuery request, CancellationToken cancellationToken) =>
        await blobService.DownloadFileAsync(request.FileId, BlobContainerName.Files, cancellationToken);
}
