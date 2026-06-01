using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.Invoices.Commands;
using Application.Invoices.Queries;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Sites.Invoices;

public class Invoices : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroupCustom(customGroupName: "sites/{siteId:guid}/invoices");
        group.MapPost("/", UploadInvoice).DisableAntiforgery();
        // TODO: Re-enable authorization after Scalar/API exploration is complete.
        group.MapGet("/{invoiceId:guid}", GetInvoiceById);
        group.MapGet("/", GetInvoices);
        group.MapPatch("/{invoiceId:guid}/approve", ApproveInvoice);
        group.MapPatch("/{invoiceId:guid}/process", ProcessInvoice);
    }

    private static async Task<IResult> UploadInvoice(IMediator mediator, Guid siteId, [FromForm] IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        var result = await mediator.Send(new UploadInvoiceCommand
        {
            SiteId = siteId,
            Stream = stream,
            FileName = Path.GetFileName(file.FileName),
            ContentType = file.ContentType,
            ContentLength = file.Length
        });

        return TypedResults.Created(
            $"/sites/{siteId}/invoices/{result.InvoiceDocumentId}",
            result);
    }

    private static async Task<Results<Ok<InvoiceDetailsDto>, NotFound>> GetInvoiceById(
        IMediator mediator,
        Guid siteId,
        Guid invoiceId)
    {
        var invoice = await mediator.Send(new GetInvoiceByIdQuery { SiteId = siteId, InvoiceId = invoiceId });

        return invoice is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(invoice);
    }

    private static async Task<Ok<List<InvoiceSummaryDto>>> GetInvoices(IMediator mediator, Guid siteId)
    {
        var invoices = await mediator.Send(new GetInvoicesQuery { SiteId = siteId });
        return TypedResults.Ok(invoices);
    }

    private static async Task<NoContent> ApproveInvoice(IMediator mediator, Guid siteId, Guid invoiceId)
    {
        await mediator.Send(new ApproveInvoiceCommand { SiteId = siteId, InvoiceId = invoiceId });
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> ProcessInvoice(IMediator mediator, Guid siteId, Guid invoiceId)
    {
        await mediator.Send(new ProcessInvoiceCommand { SiteId = siteId, InvoiceId = invoiceId });
        return TypedResults.NoContent();
    }
}
