using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.Sites.Images.Commands;
using Application.Sites.Images.Queries;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Sites.Images;

public class Images : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        // var group = app.MapGroupCustom().RequireAuthorization();
        var group = app.MapGroupCustom();
        group.MapPost("/", AddImageToSite).DisableAntiforgery();
        group.MapGet("/{imageId:guid}", GetImageFromSite);
        group.MapDelete("/{imageId:guid}", DeleteImageFromSite);
    }

    private static async Task<FileStreamHttpResult> GetImageFromSite(IMediator mediator, Guid imageId)
    {
        var fileResponse = await mediator.Send(new GetImageQuery { FileId = imageId });
        return TypedResults.File(fileResponse.Stream, fileResponse.ContentType);
    }

    private static async Task<NoContent> DeleteImageFromSite(IMediator mediator, Guid imageId)
    {
        await mediator.Send(new DeleteImageCommand { FileId = imageId });
        return TypedResults.NoContent();
    }

    private static async Task<Ok<Guid>> AddImageToSite(IMediator mediator, [FromForm] IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        var uploadedFile = new UploadedFile { Stream = stream, ContentType = file.ContentType };
        var fileId = await mediator.Send(new AddImageCommand { File = uploadedFile });

        return TypedResults.Ok(fileId);
    }
}