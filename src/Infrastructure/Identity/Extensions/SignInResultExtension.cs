using Application.Identity.Commands;
using Application.SeedWork.Models;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Extensions;

public static class SignInResultExtension
{
    public static Result ToApplicationResult(this SignInResult signInResult)
    {
        return signInResult.Succeeded
            ? Result.Success()
            : Result.Failure([IdentityResultErrors.InvalidCredentials]);
    }
}
