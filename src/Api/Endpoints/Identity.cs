using Api.SeedWork;
using Application.Identity;
using Application.Identity.Commands.SignIn;
using Application.Identity.Commands.SignUp;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public class Identity : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this.GetType().Name);

        group.MapPost("/signUp", SignUp);
        group.MapPost("/signIn", SignIn);
    }

    public async Task<Results<Ok<IdentityResultWithToken>, BadRequest<string[]>>> SignUp(
        IMediator mediator,
        [FromBody] SignUpCommand command
    )
    {
        var result = await mediator.Send(command);
        if (result.Result.Succeeded && result is IdentityResultWithToken resultWithToken)
        {
            return TypedResults.Ok(resultWithToken);
        }

        return TypedResults.BadRequest(result.Result.Errors);
    }

    public async Task<Results<Ok<string>, BadRequest<string[]>>> SignIn(
        IMediator mediator,
        [FromBody] SignInCommand command
    )
    {
        var result = await mediator.Send(command);
        if (result.Result.Succeeded && result is IdentityResultWithToken resultWithToken)
        {
            return TypedResults.Ok(resultWithToken.Token);
        }

        return TypedResults.BadRequest(result.Result.Errors);
    }
}
