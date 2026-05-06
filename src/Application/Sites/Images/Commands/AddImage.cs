using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.Internal;
using MediatR;

namespace Application.Sites.Images.Commands;

public sealed record AddImageCommand : IRequest<UploadedImageResult>
{
    public required UploadedFile File { get; init; }
}

public class AddImageHandler(IBlobService blobService) : IRequestHandler<AddImageCommand, UploadedImageResult>
{
    public Task<UploadedImageResult> Handle(AddImageCommand request, CancellationToken cancellationToken)
        => blobService.UploadAsync(request.File.Stream, request.File.ContentType, BlobContainerName.Images,
            cancellationToken);
}