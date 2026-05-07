using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.Internal;
using MediatR;

namespace Application.Sites.Images.Commands;

public sealed record AddImageCommand : IRequest<UploadedImageResult>
{
    public required Guid SiteId { get; init; }
    public required UploadedFile File { get; init; }
}

public class AddImageHandler(IBlobService blobService, IImagesService imagesService)
    : IRequestHandler<AddImageCommand, UploadedImageResult>
{
    public async Task<UploadedImageResult> Handle(AddImageCommand request, CancellationToken cancellationToken)
    {
        var result = await blobService.UploadImageAsync(request.File.Stream, request.File.ContentType,
            BlobContainerName.Images,
            cancellationToken);

        await imagesService.AddImageIdsToSiteAsync(request.SiteId, result.OriginalFileId, result.ThumbnailFileId,
            cancellationToken);

        return result;
    }
}