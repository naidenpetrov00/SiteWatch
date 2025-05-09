using Application.Identity.Commands;
using Application.SeedWork.Enums;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Infrastructure.Identity.Extensions;
using Infrastructure.Identity.Extensions.cs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService,
        IJwtTokenService jwtTokenService,
        SignInManager<ApplicationUser> signInManager
    )
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
        _jwtTokenService = jwtTokenService;
        _signInManager = signInManager;
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

    public async Task<ApplicationUser?> FindUserByEmailAsync(string email) =>
        await _userManager.FindByEmailAsync(email);

    public async Task<IdentityResultModel> CheckPasswordAsync(ApplicationUser user, string password)
    {
        var result = await _signInManager.PasswordSignInAsync(user, password, false, false);

        if (result.Succeeded)
        {
            var token = _jwtTokenService.GenerateToken(user);
            return new IdentityResultWithToken
            {
                Result = result.ToApplicationResult(),
                Token = token,
            };
        }

        return new IdentityResultOnly { Result = result.ToApplicationResult() };
    }

    public async Task<IdentityResultModel> ConfirmEmailAsync(ApplicationUser user, string token)
    {
        var authenticatoinToken = await _userManager.GetAuthenticationTokenAsync(
            user,
            EmailProvider.Email.ToString(),
            EmailProvider.SMTP.ToString()
        );

        if (authenticatoinToken == null || authenticatoinToken != token)
            return new IdentityResultOnly
            {
                Result = Result.Failure([IdentityResultErrors.TokenNotValid]),
            };

        var secureToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var result = await _userManager.ConfirmEmailAsync(user, secureToken);
        if (result.Succeeded)
        {
            await _userManager.RemoveAuthenticationTokenAsync(
                user,
                EmailProvider.Email.ToString(),
                EmailProvider.SMTP.ToString()
            );
            return new IdentityResultOnly { Result = Result.Success() };
        }
        return new IdentityResultOnly { Result = result.ToApplicationResult() };
    }

    public string GenerateVerificationToken() => new Random().Next(100000, 999999).ToString("D6");

    public async Task<IdentityResultModel> ResetPasswordAsync(
        ApplicationUser user,
        string token,
        string newPassword
    )
    {
        var authenticatoinToken = await _userManager.GetAuthenticationTokenAsync(
            user,
            EmailProvider.Password.ToString(),
            EmailProvider.SMTP.ToString()
        );

        if (authenticatoinToken == null || authenticatoinToken != token)
            return new IdentityResultOnly
            {
                Result = Result.Failure([IdentityResultErrors.TokenNotValid]),
            };

        var identityPasswordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(
            user,
            identityPasswordResetToken,
            newPassword
        );
        if (result.Succeeded)
        {
            await _userManager.RemoveAuthenticationTokenAsync(
                user,
                EmailProvider.Password.ToString(),
                EmailProvider.SMTP.ToString()
            );
            return new IdentityResultOnly { Result = Result.Success() };
        }
        return new IdentityResultOnly { Result = result.ToApplicationResult() };
    }
}
