using Application.Identity.Commands;
using Application.Identity.Queries.Users;
using Application.SeedWork.Enums;
using Application.SeedWork.Models;
using Application.SeedWork.Security;
using Domain.Entities;
using Infrastructure.Identity.Extensions;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Services;

public class IdentityVerificationService(
    UserManager<ApplicationUser> userManager,
    IJwtTokenService jwtTokenService
) : IIdentityVerificationService
{
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
        {
            return new IdentityResultOnly
            {
                Result = Result.Failure([IdentityResultErrors.TokenNotValid]),
            };
        }

        var secureToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var result = await userManager.ConfirmEmailAsync(user, secureToken);

        if (!result.Succeeded)
        {
            return new IdentityResultOnly
            {
                Result = Result.Failure([IdentityResultErrors.TokenNotValid]),
            };
        }

        await userManager.RemoveAuthenticationTokenAsync(
            user,
            EmailProvider.Email.ToString(),
            EmailProvider.SMTP.ToString()
        );

        var token = await jwtTokenService.GenerateTokenAsync(user);
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

    public string GenerateVerificationToken() =>
        new Random().Next(100000, 999999).ToString("D6");

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
        {
            return new IdentityResultOnly
            {
                Result = Result.Failure([IdentityResultErrors.TokenNotValid]),
            };
        }

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
