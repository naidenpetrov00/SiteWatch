using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.Sites.Files.Commands;
using Application.Sites.Files.Queries;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Sites.Files;

public class Files : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroupCustom();
        group.MapPost("/{siteId:guid}", AddFileToSite).DisableAntiforgery();
        group.MapGet("/{fileId:guid}", GetFileFromSite);
        group.MapDelete("/{fileId:guid}", DeleteFileFromSite);
        group.MapGet("/files{siteId:guid}", GetFilesIdsBySiteId);
    }

    private static async Task<FileStreamHttpResult> GetFileFromSite(IMediator mediator, Guid fileId)
    {
        var fileResponse = await mediator.Send(new GetFileQuery { FileId = fileId });
        return TypedResults.File(fileResponse.Stream, fileResponse.ContentType);
    }

    private static async Task<NoContent> DeleteFileFromSite(IMediator mediator, Guid fileId)
    {
        await mediator.Send(new DeleteFileCommand { FileId = fileId });
        return TypedResults.NoContent();
    }

    private static async Task<Ok<UploadedFileResult>> AddFileToSite(
        IMediator mediator,
        [FromForm] IFormFile file,
        Guid siteId)
    {
        await using var stream = file.OpenReadStream();
        var uploadedFile = new UploadedFile
        {
            Stream = stream,
            ContentType = file.ContentType,
            FileName = Path.GetFileName(file.FileName),
        };

        var fileId = await mediator.Send(new AddFileCommand(siteId, uploadedFile));

        return TypedResults.Ok(fileId);
    }

    private static async Task<Ok<List<SiteFileIdsDto>>> GetFilesIdsBySiteId(IMediator mediator, Guid siteId)
    {
        var filesIds = await mediator.Send(new GetFilesIdsBySiteIdQuery { SiteId = siteId });
        return TypedResults.Ok(filesIds);
    }
}
