using Application.Identity.Commands;
using Application.Identity.Queries.Users;
using Application.SeedWork.Enums;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Identity.Extensions;
using Infrastructure.Identity.Extensions.cs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Services;

public class IdentityService(
    UserManager<ApplicationUser> userManager,
    IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
    IAuthorizationService authorizationService,
    IJwtTokenService jwtTokenService,
    SignInManager<ApplicationUser> signInManager,
    IEmailService emailService,
    IMapper mapper)
    : IIdentityService
{
    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<IdentityResultModel> CreateUserAsync(
        string userName,
        string email,
        string password
    )
    {
        var user = new ApplicationUser { UserName = userName, Email = email };

        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            return new IdentityResultOnly { Result = result.ToApplicationResult() };
        }

        var emailVerificationToken = GenerateVerificationToken();
        await emailService.SendVerifyEmailAsync(user, user.Email!, emailVerificationToken);

        return new IdentityResultWithEmail { Result = result.ToApplicationResult(), Email = email };
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await userManager.FindByIdAsync(userId);

        return user != null && await userManager.IsInRoleAsync(user, role);
    }

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

    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);

        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        var result = await userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }

    public async Task<ApplicationUser?> FindUserByEmailAsync(string email) =>
        await userManager.FindByEmailAsync(email);

    public async Task<IdentityResultModel> CheckPasswordAsync(ApplicationUser user, string password)
    {
        var result = await signInManager.PasswordSignInAsync(user, password, false, false);

        if (result.Succeeded)
        {
            var token = jwtTokenService.GenerateToken(user);
            var userDto = mapper.Map<UserInfoDto>(user);
            return new IdentityResultWithUserToken
            {
                Result = result.ToApplicationResult(),
                Token = token,
                User = userDto,
            };
        }

        return new IdentityResultOnly { Result = result.ToApplicationResult() };
    }

    public async Task<IdentityResultModel> ConfirmEmailAsync(
        ApplicationUser user,
        string emailToken
    )
    {
        var authenticatoinToken = await userManager.GetAuthenticationTokenAsync(
            user,
            EmailProvider.Email.ToString(),
            EmailProvider.SMTP.ToString()
        );

        if (authenticatoinToken == null || authenticatoinToken != emailToken)
            return new IdentityResultOnly
            {
                Result = Result.Failure([IdentityResultErrors.TokenNotValid]),
            };

        var secureToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var result = await userManager.ConfirmEmailAsync(user, secureToken);
        if (result.Succeeded)
        {
            await userManager.RemoveAuthenticationTokenAsync(
                user,
                EmailProvider.Email.ToString(),
                EmailProvider.SMTP.ToString()
            );
            var token = jwtTokenService.GenerateToken(user);
            return new IdentityResultWithUserToken
            {
                Result = Result.Success(),
                User = new UserInfoDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                },
                Token = token,
            };
        }
        return new IdentityResultOnly
        {
            Result = Result.Failure([IdentityResultErrors.TokenNotValid]),
        };
    }

    public string GenerateVerificationToken() => new Random().Next(100000, 999999).ToString("D6");

    public async Task<IdentityResultModel> ResetPasswordAsync(
        ApplicationUser user,
        string token,
        string newPassword
    )
    {
        var authenticatoinToken = await userManager.GetAuthenticationTokenAsync(
            user,
            EmailProvider.Password.ToString(),
            EmailProvider.SMTP.ToString()
        );

        if (authenticatoinToken == null || authenticatoinToken != token)
            return new IdentityResultOnly
            {
                Result = Result.Failure([IdentityResultErrors.TokenNotValid]),
            };

        var identityPasswordResetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(
            user,
            identityPasswordResetToken,
            newPassword
        );
        if (result.Succeeded)
        {
            await userManager.RemoveAuthenticationTokenAsync(
                user,
                EmailProvider.Password.ToString(),
                EmailProvider.SMTP.ToString()
            );
            return new IdentityResultOnly { Result = Result.Success() };
        }
        return new IdentityResultOnly { Result = result.ToApplicationResult() };
    }

    public async Task<bool> IsVerifiedEmailAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return false;
        }

        return await userManager.IsEmailConfirmedAsync(user);
    }
}
