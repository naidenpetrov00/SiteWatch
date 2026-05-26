using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.Sites.Videos.Commands;
using Application.Sites.Videos.Queries;
using Domain.SeedWork.Enums;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Sites.Videos;

public class Videos : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroupCustom();
        group.MapPost("/{siteId:guid}", AddVideoToSite).DisableAntiforgery();
        group.MapGet("/{videoId:guid}", GetVideoFromSite);
        group.MapGet("/snapshot/{snapshotId:guid}", GetVideoSnapshotFromSite);
        group.MapDelete("/{videoId:guid}", DeleteVideoFromSite);
        group.MapGet("/site/{siteId:guid}", GetVideosIdsBySiteId);
    }

    private static async Task<FileStreamHttpResult> GetVideoFromSite(IMediator mediator, Guid videoId)
    {
        var fileResponse = await mediator.Send(new GetVideoQuery { FileId = videoId });
        return TypedResults.File(fileResponse.Stream, fileResponse.ContentType);
    }

    private static async Task<FileStreamHttpResult> GetVideoSnapshotFromSite(IMediator mediator, Guid snapshotId)
    {
        var fileResponse = await mediator.Send(new GetVideoSnapshotQuery { SnapshotId = snapshotId });
        return TypedResults.File(fileResponse.Stream, fileResponse.ContentType);
    }

    private static async Task<NoContent> DeleteVideoFromSite(IMediator mediator, Guid videoId)
    {
        await mediator.Send(new DeleteVideoCommand { FileId = videoId });
        return TypedResults.NoContent();
    }

    private static async Task<Ok<UploadedVideoResult>> AddVideoToSite(
        IMediator mediator,
        [FromForm] IFormFile file,
        [FromForm] VideoCategory? category,
        Guid siteId)
    {
        await using var stream = file.OpenReadStream();
        var uploadedFile = new UploadedFile { Stream = stream, ContentType = file.ContentType };
        var fileId = await mediator.Send(new AddVideoCommand(siteId, uploadedFile, category));

        return TypedResults.Ok(fileId);
    }

    private static async Task<Ok<List<SiteVideoIdsDto>>> GetVideosIdsBySiteId(IMediator mediator, Guid siteId)
    {
        var videosIds = await mediator.Send(new GetVideosIdsBySiteIdQuery { SiteId = siteId });
        return TypedResults.Ok(videosIds);
    }
}
