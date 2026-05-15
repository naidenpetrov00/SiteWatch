using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.Internal;
using Domain.SeedWork.Enums;
using MediatR;

namespace Application.Sites.Images.Commands;

public sealed record AddImageCommand(
    Guid SiteId,
    UploadedFile File,
    ImageCategory Category = ImageCategory.Other)
    : IRequest<UploadedImageResult>;

public class AddImageHandler(IBlobService blobService, IImagesService imagesService)
    : IRequestHandler<AddImageCommand, UploadedImageResult>
{
    public async Task<UploadedImageResult> Handle(AddImageCommand request, CancellationToken cancellationToken)
    {
        var result = await blobService.UploadImageAsync(request.File.Stream, request.File.ContentType,
            BlobContainerName.Images,
            cancellationToken);

        await imagesService.AddImageIdsToSiteAsync(request.SiteId, result.OriginalFileId, result.ThumbnailFileId,
            request.Category,
            cancellationToken);

        return result;
    }
}