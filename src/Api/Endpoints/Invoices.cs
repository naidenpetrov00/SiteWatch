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
        var dashboardGroup = app
            .MapGroupCustom(customGroupName: "dashboard")
            .RequireAuthorization(AuthorizationPolicies.Administrator);

        group.MapPost(string.Empty, CreateInvoice);
        group.MapPut("/{invoiceId:guid}/site-allocations", UpdateInvoiceSiteAllocations);
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

    private static async Task<Ok<PagedResult<DashboardInvoiceDto>>> GetDashboardInvoices(
        IMediator mediator,
        [AsParameters] DashboardInvoicesQuery query
    )
    {
        var invoices = await mediator.Send(query);
        return TypedResults.Ok(invoices);
    }
}
