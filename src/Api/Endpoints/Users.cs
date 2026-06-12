using Api.SeedWork;
using Api.SeedWork.EndpointFilters;
using Api.SeedWork.Extensions;
using Application.Identity.Commands;
using Application.Identity.Queries.DashboardUsers;
using Application.Identity.Queries.Users;
using Application.SeedWork.Models;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroupCustom();
        var dashboardGroup = app.MapGroupCustom(customGroupName: "dashboard");

        group.MapGet("/{email}", GetUserByEmail).AddEndpointFilter<AuthorizationFilter>();
        dashboardGroup.MapGet("/users", GetDashboardUsers).AddEndpointFilter<AuthorizationFilter>();
    }

    public async Task<Results<Ok<IdentityResultWithUser>, BadRequest<string[]>>> GetUserByEmail(
        IMediator mediator,
        string email
    )
    {
        var query = new UsersQuery { Email = email };

        var result = await mediator.Send(query);
        if (result.Result.Succeeded && result is IdentityResultWithUser resultWithUser)
        {
            return TypedResults.Ok(resultWithUser);
        }
        return TypedResults.BadRequest(result.Result.Errors);
    }

private static async Task<Ok<PagedResult<DashboardUserDto>>> GetDashboardUsers(
        IMediator mediator,
        [AsParameters] DashboardUsersQuery query
    )
    {
        var users = await mediator.Send(query);

        return TypedResults.Ok(users);
    }
}
