using Api.SeedWork;
using Application.Identity;
using Application.Identity.Commands.SignUp;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public class Identity : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this.GetType().Name);

        group.MapGet("/signUp", SignUp);
    }

    private async Task<Results<Ok<string>, BadRequest<string[]>>> SignUp(
        IMediator mediator,
        SignUpCommand command
    )
    {
        var result = await mediator.Send(command);
        if (result.Result.Succeeded && result is IdentityResultWithToken resultWithToken)
        {
            return TypedResults.Ok(resultWithToken.Token);
        }

        return TypedResults.BadRequest(result.Result.Errors);
    }

    private async Task<Results<Ok<string>, BadRequest<string[]>>> SignIn(
        IMediator mediator,
        SignInCommand command
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
