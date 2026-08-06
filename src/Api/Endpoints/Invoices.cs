using Api.SeedWork;
using Api.SeedWork.Extensions;
using Api.Services;
using Application.Invoices.Commands;
using Application.Invoices.Queries;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Application.SeedWork.Security;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Api.Endpoints;

public class Invoices : EndpointGroupBase
{
    private static readonly TimeSpan FileAccessLifetime = TimeSpan.FromMinutes(5);

    public override void Map(WebApplication app)
    {
        var group = app
            .MapGroupCustom(customGroupName: "invoices")
            .RequireAuthorization(AuthorizationPolicies.Administrator);
        var siteGroup = app
            .MapGroupCustom(customGroupName: "invoices")
            .RequireAuthorization(AuthorizationPolicies.AdministratorOrWorker);
        var dashboardGroup = app
            .MapGroupCustom(customGroupName: "dashboard")
            .RequireAuthorization(AuthorizationPolicies.Administrator);
        var fileAccessGroup = app.MapGroupCustom(customGroupName: "invoices");

        group.MapPost(string.Empty, CreateInvoice);
        group.MapPut("/{invoiceId:guid}", UpdateInvoice);
        group.MapPut("/{invoiceId:guid}/site-allocations", UpdateInvoiceSiteAllocations);
        group.MapPut("/{invoiceId:guid}/file", UploadInvoiceFile)
            .DisableAntiforgery()
            .WithName("UploadInvoiceFile")
            .WithSummary("Upload or replace an invoice file")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status413PayloadTooLarge)
            .Produces(StatusCodes.Status404NotFound);
        siteGroup.MapPost("/site/{siteId:guid}/file", CreateInvoiceFromFile)
            .DisableAntiforgery()
            .WithName("CreateInvoiceFromFile")
            .WithSummary("Create an incomplete invoice from an uploaded file")
            .Produces<CreateInvoiceFromFileResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status413PayloadTooLarge);
        siteGroup.MapGet("/site/{siteId:guid}", GetSiteInvoices)
            .WithName("GetSiteInvoices")
            .WithSummary("Get invoices allocated to an assigned site")
            .Produces<IReadOnlyList<SiteInvoiceDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        siteGroup.MapGet(
                "/site/{siteId:guid}/{invoiceId:guid}/file-access",
                GetInvoiceFileAccess)
            .WithName("GetInvoiceFileAccess")
            .WithSummary("Get temporary read access to an invoice file")
            .Produces<InvoiceFileAccessDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        fileAccessGroup.MapGet("/file", DownloadInvoiceFile)
            .AllowAnonymous()
            .WithName("DownloadInvoiceFile")
            .WithSummary("Open an invoice file using temporary access")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        dashboardGroup.MapGet("/invoices", GetDashboardInvoices);
    }

    private static async Task<IResult> CreateInvoice(
        IMediator mediator,
        CreateInvoiceCommand command,
        CancellationToken cancellationToken)
    {
        var invoiceId = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/invoices/{invoiceId}", new { id = invoiceId });
    }

    private static async Task<NoContent> UpdateInvoiceSiteAllocations(
        IMediator mediator,
        Guid invoiceId,
        UpdateInvoiceSiteAllocationsCommand command,
        CancellationToken cancellationToken)
    {
        command.InvoiceId = invoiceId;
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> UpdateInvoice(
        IMediator mediator,
        Guid invoiceId,
        UpdateInvoiceCommand command,
        CancellationToken cancellationToken)
    {
        command.InvoiceId = invoiceId;
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    [RequestSizeLimit(InvoiceFileValidation.MaxRequestSize)]
    [RequestFormLimits(MultipartBodyLengthLimit = InvoiceFileValidation.MaxRequestSize)]
    private static async Task<NoContent> UploadInvoiceFile(
        IMediator mediator,
        Guid invoiceId,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var uploadedFile = new UploadedInvoiceFile(
            stream,
            Path.GetFileName(file.FileName),
            file.ContentType,
            file.Length);

        await mediator.Send(
            new UploadInvoiceFileCommand(invoiceId, uploadedFile),
            cancellationToken);
        return TypedResults.NoContent();
    }

    [RequestSizeLimit(InvoiceFileValidation.MaxRequestSize)]
    [RequestFormLimits(MultipartBodyLengthLimit = InvoiceFileValidation.MaxRequestSize)]
    private static async Task<Created<CreateInvoiceFromFileResponse>> CreateInvoiceFromFile(
        IMediator mediator,
        Guid siteId,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var uploadedFile = new UploadedInvoiceFile(
            stream,
            Path.GetFileName(file.FileName),
            file.ContentType,
            file.Length);
        var invoiceId = await mediator.Send(
            new CreateInvoiceFromFileCommand(siteId, uploadedFile),
            cancellationToken);

        return TypedResults.Created(
            $"/invoices/{invoiceId}",
            new CreateInvoiceFromFileResponse(invoiceId));
    }

    private static async Task<Ok<IReadOnlyList<SiteInvoiceDto>>> GetSiteInvoices(
        IMediator mediator,
        Guid siteId,
        CancellationToken cancellationToken)
    {
        var invoices = await mediator.Send(
            new SiteInvoicesQuery(siteId),
            cancellationToken);
        return TypedResults.Ok(invoices);
    }

    private static async Task<Ok<InvoiceFileAccessDto>> GetInvoiceFileAccess(
        IMediator mediator,
        IInvoiceFileAccessTicketService ticketService,
        IUser user,
        HttpContext httpContext,
        Guid siteId,
        Guid invoiceId,
        CancellationToken cancellationToken)
    {
        var fileInfo = await mediator.Send(
            new InvoiceFileAccessQuery(siteId, invoiceId),
            cancellationToken);
        var expiresAt = DateTimeOffset.UtcNow.Add(FileAccessLifetime);
        var ticket = ticketService.Create(
            siteId,
            invoiceId,
            user.Id ?? throw new UnauthorizedAccessException(),
            expiresAt);
        var relativeUrl = QueryHelpers.AddQueryString(
            "/invoices/file",
            "ticket",
            ticket);

        SetSensitiveResponseHeaders(httpContext.Response);

        return TypedResults.Ok(new InvoiceFileAccessDto(
            relativeUrl,
            fileInfo.FileName,
            fileInfo.ContentType,
            expiresAt));
    }

    private static async Task<Results<FileStreamHttpResult, NotFound>> DownloadInvoiceFile(
        IMediator mediator,
        IInvoiceFileAccessTicketService ticketService,
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
            new InvoiceFileDownloadQuery(
                accessTicket.SiteId,
                accessTicket.InvoiceId,
                accessTicket.UserId),
            cancellationToken);

        httpContext.Response.ContentLength = file.ContentLength;
        httpContext.Response.GetTypedHeaders().ContentDisposition =
            new ContentDispositionHeaderValue("inline")
            {
                FileNameStar = file.FileName
            };

        return TypedResults.File(file.Stream, file.ContentType);
    }

    private static void SetSensitiveResponseHeaders(HttpResponse response)
    {
        response.Headers.CacheControl = "private, no-store";
        response.Headers["Content-Security-Policy"] = "sandbox";
        response.Headers["Referrer-Policy"] = "no-referrer";
        response.Headers["X-Content-Type-Options"] = "nosniff";
    }

    private static async Task<Ok<PagedResult<DashboardInvoiceDto>>> GetDashboardInvoices(
        IMediator mediator,
        [AsParameters] DashboardInvoicesQuery query
    )
    {
        var invoices = await mediator.Send(query);
        return TypedResults.Ok(invoices);
    }

    public sealed record CreateInvoiceFromFileResponse(Guid Id);
}
