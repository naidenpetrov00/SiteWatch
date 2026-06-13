using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.Persons.Commands;
using Application.Persons.Queries;
using Application.SeedWork.Models;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public class Persons : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroupCustom(customGroupName: "persons").RequireAuthorization();
        var dashboardGroup = app.MapGroupCustom(customGroupName: "dashboard").RequireAuthorization();

        group.MapPost(string.Empty, CreatePerson);
        group.MapDelete("/{personId:guid}", DeletePerson);
        dashboardGroup.MapGet("/persons", GetDashboardPersons);
    }

    private static async Task<IResult> CreatePerson(IMediator mediator, CreatePersonCommand command)
    {
        var personId = await mediator.Send(command);
        return TypedResults.Created($"/persons/{personId}", new { id = personId });
    }

    private static async Task<NoContent> DeletePerson(IMediator mediator, Guid personId)
    {
        await mediator.Send(new DeletePersonCommand { Id = personId });
        return TypedResults.NoContent();
    }

    private static async Task<Ok<PagedResult<PersonTableDto>>> GetDashboardPersons(
        IMediator mediator,
        [AsParameters] DashboardPersonsQuery query
    )
    {
        var persons = await mediator.Send(query);
        return TypedResults.Ok(persons);
    }
}
