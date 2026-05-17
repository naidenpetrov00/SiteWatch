using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.Internal;
using MediatR;

namespace Application.Sites.Videos.Commands;

public sealed record DeleteVideoCommand : IRequest
{
    public Guid FileId { get; init; }
}

public class DeleteVideoHandler(IVideosBlobService blobService) : IRequestHandler<DeleteVideoCommand>
{
    public async Task Handle(DeleteVideoCommand request, CancellationToken cancellationToken)
    {
        await blobService.DeleteVideoAsync(request.FileId, BlobContainerName.Videos, cancellationToken);
    }
}
