using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.Invoices.Commands;
using Application.Invoices.Queries;
using Application.SeedWork.Models;
using Application.SeedWork.Security;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public class Invoices : EndpointGroupBase
{
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

        group.MapPost(string.Empty, CreateInvoice);
        group.MapPut("/{invoiceId:guid}/site-allocations", UpdateInvoiceSiteAllocations);
        group.MapPut("/{invoiceId:guid}/file", UploadInvoiceFile)
            .DisableAntiforgery()
            .WithName("UploadInvoiceFile")
            .WithSummary("Upload or replace an invoice file")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
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
            file.ContentType);

        await mediator.Send(
            new UploadInvoiceFileCommand(invoiceId, uploadedFile),
            cancellationToken);
        return TypedResults.NoContent();
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
        Guid siteId,
        Guid invoiceId,
        CancellationToken cancellationToken)
    {
        var access = await mediator.Send(
            new InvoiceFileAccessQuery(siteId, invoiceId),
            cancellationToken);
        return TypedResults.Ok(access);
    }

    private static async Task<Ok<PagedResult<DashboardInvoiceDto>>> GetDashboardInvoices(
        IMediator mediator,
        [AsParameters] DashboardInvoicesQuery query
    )
    {
        var invoices = await mediator.Send(query);
        return TypedResults.Ok(invoices);
    }
}
