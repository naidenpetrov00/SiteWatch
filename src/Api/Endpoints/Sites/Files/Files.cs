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
        // var group = app.MapGroupCustom().RequireAuthorization();
        var group = app.MapGroupCustom();
        group.MapPost("/", AddFileToSite).DisableAntiforgery();
        group.MapGet("/{fileId:guid}", GetFileFromSite);
        group.MapDelete("/{fileId:guid}", DeleteFileFromSite);
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

    private static async Task<Ok<Guid>> AddFileToSite(IMediator mediator, [FromForm] IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        var uploadedFile = new UploadedFile { Stream = stream, ContentType = file.ContentType };
        var fileId = await mediator.Send(new AddFileCommand { File = uploadedFile });

        return TypedResults.Ok(fileId);
    }
}