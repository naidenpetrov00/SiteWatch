using Api.SeedWork;
using Api.Services;
using Application.Proposals;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Api.Endpoints;

public sealed class Proposals : EndpointGroupBase
{
    private static readonly TimeSpan PdfAccessLifetime = TimeSpan.FromMinutes(5);

    public override void Map(WebApplication app)
    {
        var group = app
            .MapGroup("/sites/{siteId:guid}")
            .RequireAuthorization(AuthorizationPolicies.Administrator)
            .WithTags("Proposals");

        group.MapPost("/offers/{offerId:guid}/proposals", CreateProposal)
            .WithName("CreateOfferProposal")
            .WithSummary("Create the next draft Proposal revision from a finalized Offer")
            .Produces<ProposalCreatedResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        group.MapGet("/offers/{offerId:guid}/proposals", GetProposalHistory)
            .WithName("GetOfferProposalHistory")
            .WithSummary("Get Proposal revision history for an Offer")
            .Produces<IReadOnlyList<ProposalSummaryDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        group.MapGet("/proposals/{proposalId:guid}", GetProposal)
            .WithName("GetProposal")
            .WithSummary("Get a Proposal commercial snapshot")
            .Produces<ProposalDetailsDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        group.MapPut("/proposals/{proposalId:guid}", UpdateProposalMetadata)
            .WithName("UpdateProposalMetadata")
            .WithSummary("Update editable metadata on a draft Proposal")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        group.MapPatch("/proposals/{proposalId:guid}/issue", IssueProposal)
            .WithName("IssueProposal")
            .WithSummary("Issue a Proposal after storing its immutable PDF")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        group.MapGet("/proposals/{proposalId:guid}/pdf-access", GetProposalPdfAccess)
            .WithName("GetProposalPdfAccess")
            .WithSummary("Get temporary administrator access to an issued Proposal PDF")
            .Produces<ProposalPdfAccessResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        app.MapGet("/proposals/pdf", DownloadProposalPdf)
            .AllowAnonymous()
            .WithTags("Proposals")
            .WithName("DownloadProposalPdf")
            .WithSummary("Download an issued Proposal PDF using temporary access")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<Created<ProposalCreatedResponse>> CreateProposal(
        IMediator mediator,
        Guid siteId,
        Guid offerId,
        CancellationToken cancellationToken)
    {
        var proposalId = await mediator.Send(
            new CreateProposalCommand(siteId, offerId),
            cancellationToken);
        return TypedResults.Created(
            $"/sites/{siteId}/proposals/{proposalId}",
            new ProposalCreatedResponse(proposalId));
    }

    private static async Task<Ok<IReadOnlyList<ProposalSummaryDto>>> GetProposalHistory(
        IMediator mediator,
        Guid siteId,
        Guid offerId,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Send(
            new ProposalHistoryQuery(siteId, offerId),
            cancellationToken));

    private static async Task<Ok<ProposalDetailsDto>> GetProposal(
        IMediator mediator,
        Guid siteId,
        Guid proposalId,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Send(
            new ProposalByIdQuery(siteId, proposalId),
            cancellationToken));

    private static async Task<NoContent> UpdateProposalMetadata(
        IMediator mediator,
        Guid siteId,
        Guid proposalId,
        UpdateProposalMetadataRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new UpdateProposalMetadataCommand(
                siteId,
                proposalId,
                request.ValidUntil,
                request.PublicNotes,
                request.PaymentTerms),
            cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> IssueProposal(
        IMediator mediator,
        Guid siteId,
        Guid proposalId,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new IssueProposalCommand(siteId, proposalId),
            cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<ProposalPdfAccessResponse>> GetProposalPdfAccess(
        IMediator mediator,
        IProposalPdfAccessTicketService ticketService,
        IUser user,
        HttpContext httpContext,
        Guid siteId,
        Guid proposalId,
        CancellationToken cancellationToken)
    {
        var file = await mediator.Send(
            new ProposalPdfInfoQuery(siteId, proposalId),
            cancellationToken);
        var expiresAt = DateTimeOffset.UtcNow.Add(PdfAccessLifetime);
        var ticket = ticketService.Create(
            siteId,
            proposalId,
            user.Id ?? throw new UnauthorizedAccessException(),
            expiresAt);
        var url = QueryHelpers.AddQueryString("/proposals/pdf", "ticket", ticket);
        SetSensitiveResponseHeaders(httpContext.Response);
        return TypedResults.Ok(new ProposalPdfAccessResponse(
            url,
            file.FileName,
            file.ContentType,
            expiresAt));
    }

    private static async Task<Results<FileStreamHttpResult, NotFound>> DownloadProposalPdf(
        IMediator mediator,
        IProposalPdfAccessTicketService ticketService,
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
            new ProposalPdfDownloadQuery(
                accessTicket.SiteId,
                accessTicket.ProposalId,
                accessTicket.UserId),
            cancellationToken);
        httpContext.Response.ContentLength = file.ContentLength;
        httpContext.Response.GetTypedHeaders().ContentDisposition =
            new ContentDispositionHeaderValue("attachment")
            {
                FileNameStar = file.FileName
            };
        return TypedResults.File(
            file.Stream,
            file.ContentType);
    }

    private static void SetSensitiveResponseHeaders(HttpResponse response)
    {
        response.Headers.CacheControl = "private, no-store";
        response.Headers["Content-Security-Policy"] = "sandbox";
        response.Headers["Referrer-Policy"] = "no-referrer";
        response.Headers["X-Content-Type-Options"] = "nosniff";
    }

    /// <summary>Represents the identifier of a newly created Proposal revision.</summary>
    public sealed record ProposalCreatedResponse(Guid Id);

    /// <summary>Contains editable metadata for a draft Proposal.</summary>
    public sealed record UpdateProposalMetadataRequest(
        DateOnly? ValidUntil,
        string? PublicNotes,
        string? PaymentTerms);

    /// <summary>Provides short-lived access to an issued Proposal PDF.</summary>
    public sealed record ProposalPdfAccessResponse(
        string Url,
        string FileName,
        string ContentType,
        DateTimeOffset ExpiresAt);
}
