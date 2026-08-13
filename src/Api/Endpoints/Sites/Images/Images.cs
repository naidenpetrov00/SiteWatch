using Api.SeedWork;
using Api.SeedWork.Extensions;
using Api.Endpoints.Sites;
using Application.SeedWork.Security;
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
        var group = app.MapGroupCustom();
        group.MapPost("/{siteId:guid}", AddImageToSite)
            .RequireAuthorization(AuthorizationPolicies.AdministratorOrWorker)
            .DisableAntiforgery()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status413PayloadTooLarge);
        group.MapGet("/{imageId:guid}", GetImageFromSite);
        group.MapDelete("/{imageId:guid}", DeleteImageFromSite);
        group.MapGet("/images{siteId:guid}", GetImagesIdsBySiteId);
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

    [RequestSizeLimit(MediaUploadValidation.ImageMaxRequestSize)]
    [RequestFormLimits(MultipartBodyLengthLimit = MediaUploadValidation.ImageMaxRequestSize)]
    private static async Task<Ok<UploadedImageResult>> AddImageToSite(IMediator mediator, [FromForm] IFormFile file,
        [FromForm] string? category,
        Guid siteId,
        CancellationToken cancellationToken)
    {
        var contentType = MediaUploadValidation.ValidateImage(file);
        await using var stream = file.OpenReadStream();
        var uploadedFile = new UploadedFile { Stream = stream, ContentType = contentType };
        var fileId = await mediator.Send(new AddImageCommand
            (siteId, uploadedFile, category), cancellationToken);

        return TypedResults.Ok(fileId);
    }

    private static async Task<Ok<List<SiteImageIdsDto>>> GetImagesIdsBySiteId(
        IMediator mediator,
        Guid siteId,
        CancellationToken cancellationToken)
    {
        var imagesIds = await mediator.Send(
            new GetImagesIdsBySiteIdQuery { SiteId = siteId },
            cancellationToken);
        return TypedResults.Ok(imagesIds);
    }
}
