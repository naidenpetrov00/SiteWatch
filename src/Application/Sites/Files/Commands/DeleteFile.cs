using Application.SeedWork.Interfaces;
using MediatR;

namespace Application.Sites.Files.Commands;

public sealed record DeleteFileCommand : IRequest
{
    public Guid FileId { get; init; }
}

public class DeleteFileHandler(IBlobService blobService) : IRequestHandler<DeleteFileCommand>
{
    public async Task Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        await blobService.DeleteAsync(request.FileId, cancellationToken);
    }
}