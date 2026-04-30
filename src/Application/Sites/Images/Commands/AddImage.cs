using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.Internal;
using MediatR;

namespace Application.Sites.Images.Commands;

public sealed record AddImageCommand : IRequest<Guid>
{
    public required UploadedFile File { get; init; }
}

public class AddImageHandler(IBlobService blobService) : IRequestHandler<AddImageCommand, Guid>
{
    public async Task<Guid> Handle(AddImageCommand request, CancellationToken cancellationToken)
    {
        var fileId = await blobService.UploadAsync(request.File.Stream, request.File.ContentType, BlobContainerName.Images,
            cancellationToken);

        return fileId;
    }
}