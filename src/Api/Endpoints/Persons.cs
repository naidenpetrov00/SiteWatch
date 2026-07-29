using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.Persons.Commands;
using Application.Persons.Queries;
using Application.SeedWork.Models;
using Application.SeedWork.Security;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public class Persons : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app
            .MapGroupCustom(customGroupName: "persons")
            .RequireAuthorization(AuthorizationPolicies.Administrator);
        var dashboardGroup = app
            .MapGroupCustom(customGroupName: "dashboard")
            .RequireAuthorization(AuthorizationPolicies.Administrator);

        group.MapPost(string.Empty, CreatePerson);
        group.MapGet("/{personId:guid}", GetPerson);
        group.MapPut("/{personId:guid}", UpdatePerson);
        group.MapDelete("/{personId:guid}", DeletePerson);
        dashboardGroup.MapGet("/persons/search", SearchDashboardPersons);
        dashboardGroup.MapGet("/persons", GetDashboardPersons);
    }

    private static async Task<IResult> CreatePerson(IMediator mediator, CreatePersonCommand command)
    {
        var personId = await mediator.Send(command);
        return TypedResults.Created($"/persons/{personId}", new { id = personId });
    }

    private static async Task<Ok<PersonDetailsDto>> GetPerson(IMediator mediator, Guid personId)
    {
        var person = await mediator.Send(new PersonByIdQuery { PersonId = personId });
        return TypedResults.Ok(person);
    }

    private static async Task<NoContent> UpdatePerson(IMediator mediator, Guid personId, UpdatePersonCommand command)
    {
        command.Id = personId;
        await mediator.Send(command);
        return TypedResults.NoContent();
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

    private static async Task<Ok<List<PersonLookupDto>>> SearchDashboardPersons(
        IMediator mediator,
        [AsParameters] PersonSearchQuery query
    )
    {
        var persons = await mediator.Send(query);
        return TypedResults.Ok(persons);
    }
}
