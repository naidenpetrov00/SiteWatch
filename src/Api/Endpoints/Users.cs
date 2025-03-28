using Api.SeedWork;
using Application.Identity;
using Application.Identity.Queries.Users;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this.GetType().Name);

        group.MapGet("/userByEmail", GetUserByEmail);
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
}
