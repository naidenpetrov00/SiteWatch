using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.Internal;
using Domain.SeedWork.Enums;
using MediatR;

namespace Application.Sites.Videos.Commands;

public sealed record AddVideoCommand(
    Guid SiteId,
    UploadedFile File,
    VideoCategory? Category)
    : IRequest<UploadedVideoResult>;

public class AddVideoHandler(IVideosBlobService blobService, IVideosService videosService)
    : IRequestHandler<AddVideoCommand, UploadedVideoResult>
{
    public async Task<UploadedVideoResult> Handle(AddVideoCommand request, CancellationToken cancellationToken)
    {
        var result = await blobService.UploadVideoAsync(
            request.File.Stream,
            request.File.ContentType,
            BlobContainerName.Videos,
            cancellationToken);

        await videosService.AddVideoIdsToSiteAsync(
            request.SiteId,
            result.VideoFileId,
            result.SnapshotFileId,
            result.DurationSeconds,
            request.Category!.Value,
            cancellationToken);

        return result;
    }
}
