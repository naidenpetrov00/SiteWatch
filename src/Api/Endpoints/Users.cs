using Api.SeedWork;
using Api.SeedWork.EndpointFilters;
using Api.SeedWork.Extensions;
using Application.Identity.Commands;
using Application.Identity.Queries.Users;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroupCustom();

        group.MapGet("/{email}", GetUserByEmail).AddEndpointFilter<AuthorizationFilter>();
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
