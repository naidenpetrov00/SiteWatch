using System.Security.Claims;
using Application.SeedWork.Security;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Services;

public sealed class AdministratorCreator(UserManager<ApplicationUser> userManager)
{
    public async Task CreateAsync(string userName, string email, string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            throw new InvalidOperationException(
                "Administrator creation refused because the email is already registered.");
        }

        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true,
        };

        var createResult = await userManager.CreateAsync(user, password);
        EnsureSucceeded(createResult, "create the administrator account");

        var administratorClaim = new Claim(
            UserClaimTypes.UserType,
            UserRoles.Administrator);
        var claimResult = await userManager.AddClaimAsync(user, administratorClaim);
        if (claimResult.Succeeded)
        {
            return;
        }

        await userManager.DeleteAsync(user);
        EnsureSucceeded(claimResult, "assign the administrator role");
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(
            "; ",
            result.Errors.Select(error => $"{error.Code}: {error.Description}"));
        throw new InvalidOperationException($"Failed to {operation}: {errors}");
    }
}
