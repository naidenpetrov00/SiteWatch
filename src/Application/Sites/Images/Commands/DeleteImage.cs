using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.Internal;
using MediatR;

namespace Application.Sites.Images.Commands;

public sealed record DeleteImageCommand : IRequest
{
    public Guid FileId { get; init; }
}

public class DeleteImageHandler(IBlobService blobService) : IRequestHandler<DeleteImageCommand>
{
    public async Task Handle(DeleteImageCommand request, CancellationToken cancellationToken)
    {
        await blobService.DeleteAsync(request.FileId, BlobContainerName.Images, cancellationToken);
    }
}