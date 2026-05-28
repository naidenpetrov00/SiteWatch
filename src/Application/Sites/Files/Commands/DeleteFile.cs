using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.Internal;
using MediatR;

namespace Application.Sites.Files.Commands;

public sealed record DeleteFileCommand : IRequest
{
    public Guid FileId { get; init; }
}

public class DeleteFileHandler(IFilesBlobService blobService) : IRequestHandler<DeleteFileCommand>
{
    public async Task Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        await blobService.DeleteFileAsync(request.FileId, BlobContainerName.Files, cancellationToken);
    }
}
