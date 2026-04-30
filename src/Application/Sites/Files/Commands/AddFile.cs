using Application.SeedWork.Interfaces;
using MediatR;

namespace Application.Sites.Files.Commands;

public sealed record AddFileCommand : IRequest<Guid>
{
    public required UploadedFile File { get; init; }
}

public class AddFileHandler(IBlobService blobService) : IRequestHandler<AddFileCommand, Guid>
{
    public async Task<Guid> Handle(AddFileCommand request, CancellationToken cancellationToken)
    {
        var fileId = await blobService.UploadAsync(request.File.Stream, request.File.ContentType, cancellationToken);

        return fileId;
    }
}