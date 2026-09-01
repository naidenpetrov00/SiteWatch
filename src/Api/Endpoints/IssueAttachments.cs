using Api.SeedWork;
using Api.SeedWork.Extensions;
using Api.Services;
using Application.Issues.Attachments;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Api.Endpoints;

public sealed class IssueAttachments : EndpointGroupBase
{
    private static readonly TimeSpan AccessLifetime = TimeSpan.FromMinutes(5);

    public override void Map(WebApplication app)
    {
        var group = app
            .MapGroupCustom(customGroupName: "issues")
            .RequireAuthorization(AuthorizationPolicies.Administrator);
        var contentGroup = app.MapGroupCustom(customGroupName: "issues");

        group.MapGet("/{issueId:guid}/attachments", GetAttachments)
            .WithName("GetIssueAttachments")
            .WithSummary("List issue attachments")
            .Produces<IReadOnlyList<IssueAttachmentDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        group.MapGet("/{issueId:guid}/attachments/{attachmentId:guid}", GetAttachment)
            .WithName("GetIssueAttachment")
            .WithSummary("Get issue attachment metadata")
            .Produces<IssueAttachmentDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        group.MapPost("/{issueId:guid}/attachments", AddAttachment)
            .DisableAntiforgery()
            .WithName("AddIssueAttachment")
            .WithSummary("Upload an issue attachment")
            .Produces<IssueAttachmentDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status413PayloadTooLarge);
        group.MapDelete("/{issueId:guid}/attachments/{attachmentId:guid}", DeleteAttachment)
            .WithName("DeleteIssueAttachment")
            .WithSummary("Delete an issue attachment")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
        group.MapGet("/{issueId:guid}/attachments/{attachmentId:guid}/access", GetAttachmentAccess)
            .WithName("GetIssueAttachmentAccess")
            .WithSummary("Get temporary access to issue attachment content")
            .Produces<IssueAttachmentAccessResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        contentGroup.MapGet("/attachments/content", GetAttachmentContent)
            .AllowAnonymous()
            .WithName("GetIssueAttachmentContent")
            .WithSummary("View or download issue attachment content using temporary access")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status206PartialContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<Ok<IReadOnlyList<IssueAttachmentDto>>> GetAttachments(
        IMediator mediator,
        Guid issueId,
        CancellationToken cancellationToken)
    {
        var attachments = await mediator.Send(
            new IssueAttachmentsQuery(issueId),
            cancellationToken);
        return TypedResults.Ok(attachments);
    }

    private static async Task<Ok<IssueAttachmentDto>> GetAttachment(
        IMediator mediator,
        Guid issueId,
        Guid attachmentId,
        CancellationToken cancellationToken)
    {
        var attachment = await mediator.Send(
            new IssueAttachmentQuery(issueId, attachmentId),
            cancellationToken);
        return TypedResults.Ok(attachment);
    }

    [RequestSizeLimit(IssueAttachmentValidation.MaxRequestSize)]
    [RequestFormLimits(MultipartBodyLengthLimit = IssueAttachmentValidation.MaxRequestSize)]
    private static async Task<Created<IssueAttachmentDto>> AddAttachment(
        IMediator mediator,
        Guid issueId,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        var fileName = Path.GetFileName(file.FileName.Replace('\\', '/'));
        var contentType = IssueAttachmentValidation.NormalizeContentType(file.ContentType);
        await using var stream = file.OpenReadStream();
        var attachment = await mediator.Send(
            new AddIssueAttachmentCommand(
                issueId,
                new UploadedIssueAttachment(
                    stream,
                    fileName,
                    contentType,
                    file.Length)),
            cancellationToken);

        return TypedResults.Created(
            $"/issues/{issueId}/attachments/{attachment.Id}",
            attachment);
    }

    private static async Task<NoContent> DeleteAttachment(
        IMediator mediator,
        Guid issueId,
        Guid attachmentId,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new DeleteIssueAttachmentCommand(issueId, attachmentId),
            cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<IssueAttachmentAccessResponse>, NotFound>> GetAttachmentAccess(
        IMediator mediator,
        IIssueAttachmentAccessTicketService ticketService,
        IUser user,
        HttpContext httpContext,
        Guid issueId,
        Guid attachmentId,
        bool preview,
        bool download,
        CancellationToken cancellationToken)
    {
        var attachment = await mediator.Send(
            new IssueAttachmentQuery(issueId, attachmentId),
            cancellationToken);
        if (preview && !attachment.HasPreview)
        {
            return TypedResults.NotFound();
        }

        var expiresAt = DateTimeOffset.UtcNow.Add(AccessLifetime);
        var ticket = ticketService.Create(
            issueId,
            attachmentId,
            user.Id ?? throw new UnauthorizedAccessException(),
            preview,
            !preview && (download || attachment.Kind == "File"),
            expiresAt);
        var relativeUrl = QueryHelpers.AddQueryString(
            "/issues/attachments/content",
            "ticket",
            ticket);

        SetSensitiveResponseHeaders(httpContext.Response);
        return TypedResults.Ok(new IssueAttachmentAccessResponse(relativeUrl, expiresAt));
    }

    private static async Task<Results<FileStreamHttpResult, NotFound>> GetAttachmentContent(
        IMediator mediator,
        IIssueAttachmentAccessTicketService ticketService,
        HttpContext httpContext,
        string? ticket,
        CancellationToken cancellationToken)
    {
        SetSensitiveResponseHeaders(httpContext.Response);
        if (!ticketService.TryRead(ticket ?? string.Empty, out var accessTicket)
            || accessTicket is null)
        {
            return TypedResults.NotFound();
        }

        var file = await mediator.Send(
            new IssueAttachmentContentQuery(
                accessTicket.IssueId,
                accessTicket.AttachmentId,
                accessTicket.UserId,
                accessTicket.Preview),
            cancellationToken);

        httpContext.Response.ContentLength = file.ContentLength;
        httpContext.Response.GetTypedHeaders().ContentDisposition =
            new ContentDispositionHeaderValue(accessTicket.Download ? "attachment" : "inline")
            {
                FileNameStar = file.FileName,
            };

        return TypedResults.File(
            file.Stream,
            file.ContentType,
            enableRangeProcessing: true);
    }

    private static void SetSensitiveResponseHeaders(HttpResponse response)
    {
        response.Headers.CacheControl = "private, no-store";
        response.Headers["Content-Security-Policy"] = "sandbox";
        response.Headers["Referrer-Policy"] = "no-referrer";
        response.Headers["X-Content-Type-Options"] = "nosniff";
    }

    /// <summary>Provides a short-lived URL for viewing or downloading an issue attachment.</summary>
    public sealed record IssueAttachmentAccessResponse(
        string Url,
        DateTimeOffset ExpiresAt);
}
