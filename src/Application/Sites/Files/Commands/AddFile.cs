using Application.SeedWork.Interfaces;
using Application.SeedWork.Models.Internal;
using MediatR;

namespace Application.Sites.Files.Commands;

public sealed record AddFileCommand(
    Guid SiteId,
    UploadedFile File)
    : IRequest<UploadedFileResult>;

public class AddFileHandler(IFilesBlobService blobService, IFilesService filesService)
    : IRequestHandler<AddFileCommand, UploadedFileResult>
{
    public async Task<UploadedFileResult> Handle(AddFileCommand request, CancellationToken cancellationToken)
    {
        var result = await blobService.UploadFileAsync(
            request.File.Stream,
            request.File.ContentType,
            BlobContainerName.Files,
            cancellationToken);

        await filesService.AddFileIdsToSiteAsync(
            request.SiteId,
            result.FileId,
            request.File.FileName,
            request.File.ContentType,
            cancellationToken);

        return result;
    }
}
