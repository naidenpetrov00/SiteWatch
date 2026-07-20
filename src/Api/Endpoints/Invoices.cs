using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.Invoices.Commands;
using Application.Invoices.Queries;
using Application.SeedWork.Models;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public class Invoices : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroupCustom(customGroupName: "invoices").RequireAuthorization();
        var dashboardGroup = app.MapGroupCustom(customGroupName: "dashboard").RequireAuthorization();

        group.MapPost(string.Empty, CreateInvoice);
        dashboardGroup.MapGet("/invoices", GetDashboardInvoices);
    }

    private static async Task<IResult> CreateInvoice(IMediator mediator, CreateInvoiceCommand command)
    {
        var invoiceId = await mediator.Send(command);
        return TypedResults.Created($"/invoices/{invoiceId}", new { id = invoiceId });
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
