using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.Identity.Commands;
using Application.Identity.Commands.Email;
using Application.Identity.Commands.ResetPassword;
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
        var group = app.MapGroupCustom();

        group.MapPost("/signUp", SignUp);
        group.MapPost("/signIn", SignIn);
        group.MapPost("/sendVerification", SendVerificationEmail);
        group.MapPost("/verifyEmail", VerifyEmail);
        group.MapPost("/sendResetVerification", SendResetPasswordEmail);
        group.MapPost("/resetPassword", ResetPassword);
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
            return TypedResults.Ok(resultWithToken.Token);

        return TypedResults.BadRequest(result.Result.Errors);
    }

    public async Task<Results<NoContent, BadRequest<string[]>>> SendVerificationEmail(
        IMediator mediator,
        SendEmailVerificationCommand command
    )
    {
        var result = await mediator.Send(command);
        if (result.Result.Succeeded)
            return TypedResults.NoContent();

        return TypedResults.BadRequest(result.Result.Errors);
    }

    public async Task<Results<NoContent, BadRequest<string[]>>> VerifyEmail(
        IMediator mediator,
        VerifyEmailCommand command
    )
    {
        var result = await mediator.Send(command);
        if (result.Result.Succeeded)
            return TypedResults.NoContent();

        return TypedResults.BadRequest(result.Result.Errors);
    }

    public async Task<Results<NoContent, BadRequest<string[]>>> SendResetPasswordEmail(
        IMediator mediator,
        SendResetPasswordEmailCommand command
    )
    {
        var result = await mediator.Send(command);
        if (result.Result.Succeeded)
            return TypedResults.NoContent();

        return TypedResults.BadRequest(result.Result.Errors);
    }

    public async Task<Results<NoContent, BadRequest<string[]>>> ResetPassword(
        IMediator mediator,
        ResetPasswordCommand command
    )
    {
        var result = await mediator.Send(command);
        if (result.Result.Succeeded)
            return TypedResults.NoContent();

        return TypedResults.BadRequest(result.Result.Errors);
    }
}
