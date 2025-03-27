using Application.Identity;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Infrastructure.Identity.Extensions.cs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;
    private readonly IJwtTokenService _jwtTokenService;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService,
        IJwtTokenService jwtTokenService
    )
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<IdentityResultModel> CreateUserAsync(
        string userName,
        string email,
        string password
    )
    {
        var user = new ApplicationUser { UserName = userName, Email = email };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            return new IdentityResultOnly { Result = result.ToApplicationResult() };
        }

        var token = _jwtTokenService.GenerateToken(user);
        return new IdentityResultWithToken { Result = result.ToApplicationResult(), Token = token };
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        var result = await _userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }
}
