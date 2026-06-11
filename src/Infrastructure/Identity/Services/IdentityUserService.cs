using Application.Identity.Commands;
using Application.Identity.Queries.DashboardUsers;
using Application.SeedWork.Enums;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Application.SeedWork.Security;
using Application.SeedWork.Queries;
using Domain.Entities;
using Infrastructure.Identity.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace Infrastructure.Identity.Services;

public class IdentityUserService(
    UserManager<ApplicationUser> userManager,
    IIdentityVerificationService verificationService,
    IEmailService emailService,
    IMapper mapper
) : IIdentityUserService
{
    public async Task<IdentityResultModel> AssignAdministratorClaimAsync(string userId)
    {
        var user = await FindByIdAsync(userId);

        if (user is null)
        {
            return new IdentityResultOnly
            {
                Result = Result.Failure([IdentityResultErrors.UserNotFoundById]),
            };
        }

        var claims = await userManager.GetClaimsAsync(user);
        var userTypeClaims = claims.Where(c => c.Type == UserClaimTypes.UserType).ToList();

        if (userTypeClaims.Count > 0)
        {
            var removeResult = await userManager.RemoveClaimsAsync(user, userTypeClaims);
            if (!removeResult.Succeeded)
            {
                return new IdentityResultOnly { Result = removeResult.ToApplicationResult() };
            }
        }

        var addResult = await userManager.AddClaimAsync(
            user,
            new Claim(UserClaimTypes.UserType, UserClaimTypes.Administrator)
        );
        if (!addResult.Succeeded)
        {
            return new IdentityResultOnly { Result = addResult.ToApplicationResult() };
        }

        return new IdentityResultOnly { Result = Result.Success() };
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

        var claimResult = await userManager.AddClaimAsync(
            user,
            new Claim(UserClaimTypes.UserType, UserClaimTypes.Client)
        );
        if (!claimResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return new IdentityResultOnly { Result = claimResult.ToApplicationResult() };
        }

        var emailVerificationToken = verificationService.GenerateVerificationToken();
        await emailService.SendVerifyEmailAsync(user, user.Email!, emailVerificationToken);

        return new IdentityResultWithEmail { Result = result.ToApplicationResult(), Email = email };
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await FindByIdAsync(userId);

        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        var result = await userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<PagedResult<DashboardUserDto>> GetUsersAsync(
        DashboardUsersQuery query,
        CancellationToken cancellationToken
    )
        => await userManager.Users
            .AsNoTracking()
            .ToPagedResultAsync(
                query,
                DashboardUsersQuery.Table,
                users => users.ProjectTo<DashboardUserDto>(mapper.ConfigurationProvider),
                cancellationToken
            );

    public async Task<ApplicationUser?> FindUserByEmailAsync(string email) =>
        await userManager.FindByEmailAsync(email);

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await FindByIdAsync(userId);

        if (user is null)
        {
            return false;
        }

        var claims = await userManager.GetClaimsAsync(user);

        return claims.Any(claim =>
            claim.Type == UserClaimTypes.UserType && claim.Value == role
        );
    }

    public async Task UpdateLastLoginAtAsync(ApplicationUser user)
    {
        user.LastLoginAt = DateTimeOffset.UtcNow;
        await userManager.UpdateAsync(user);
    }

    private Task<ApplicationUser?> FindByIdAsync(string userId) => userManager.FindByIdAsync(userId);
}
