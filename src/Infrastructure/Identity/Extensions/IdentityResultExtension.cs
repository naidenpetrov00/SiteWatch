using Application.SeedWork.Models;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Extensions.cs;

public static class IdentityResultExtension
{
    public static Result ToApplicationResult(this IdentityResult result) => result.Succeeded
            ? Result.Success()
            : Result.Failure(result.Errors.Select(e => e.Description));
}
