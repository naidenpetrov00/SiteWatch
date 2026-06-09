using Application.Identity.Commands;
using Application.Identity.Queries.Users;
using Application.SeedWork.Models;
using Application.SeedWork.Security;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Identity.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Services;

public class IdentityAuthenticationService(
    UserManager<ApplicationUser> userManager,
    IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
    IAuthorizationService authorizationService,
    IJwtTokenService jwtTokenService,
    SignInManager<ApplicationUser> signInManager,
    IMapper mapper,
    IIdentityUserService userService
) : IIdentityAuthenticationService
{
    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var principal = await userClaimsPrincipalFactory.CreateAsync(user);
        var result = await authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<IdentityResultModel> CheckPasswordAsync(ApplicationUser user, string password)
    {
        var result = await signInManager.PasswordSignInAsync(user, password, false, false);

        if (!result.Succeeded)
        {
            return new IdentityResultOnly { Result = result.ToApplicationResult() };
        }

        await userService.UpdateLastLoginAtAsync(user);
        return await CreateUserTokenResultAsync(user, result.ToApplicationResult());
    }

    public async Task<IdentityResultModel> CheckAdministratorPasswordAsync(
        ApplicationUser user,
        string password
    )
    {
        var claims = await userManager.GetClaimsAsync(user);
        var isAdministrator = claims.Any(
            claim =>
                claim.Type == UserClaimTypes.UserType
                && claim.Value == UserClaimTypes.Administrator
        );

        if (!isAdministrator)
        {
            return new IdentityResultOnly
            {
                Result = Result.Failure([IdentityResultErrors.InvalidCredentials]),
            };
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, password, false);

        if (!result.Succeeded)
        {
            return new IdentityResultOnly { Result = result.ToApplicationResult() };
        }

        await userService.UpdateLastLoginAtAsync(user);
        return await CreateUserTokenResultAsync(user, Result.Success());
    }

    private async Task<IdentityResultWithUserToken> CreateUserTokenResultAsync(
        ApplicationUser user,
        Result result
    )
    {
        var token = await jwtTokenService.GenerateTokenAsync(user);
        var userDto = mapper.Map<UserInfoDto>(user);

        return new IdentityResultWithUserToken
        {
            Result = result,
            Token = token,
            User = userDto,
        };
    }
}
