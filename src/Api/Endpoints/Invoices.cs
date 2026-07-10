using Api.SeedWork;
using Api.SeedWork.Extensions;
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
        var dashboardGroup = app.MapGroupCustom(customGroupName: "dashboard").RequireAuthorization();

        dashboardGroup.MapGet("/invoices", GetDashboardInvoices);
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
