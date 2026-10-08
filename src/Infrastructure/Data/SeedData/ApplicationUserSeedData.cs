using System.Security.Claims;
using Application.SeedWork.Security;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.SeedData;

public sealed class ApplicationUserSeedData(
    UserManager<ApplicationUser> userManager,
    ILogger<ApplicationUserSeedData> logger)
{
    public const string AdministratorEmail = "naiden.petrov.31.12.00@gmail.com";
    private const string ClientEmail = "naidenpetrov00@gmail.com";
    private const string DemoEmailDomain = "sitewatch.local";
    private const string DemoPassword = "Test@123";
    private const int GenericUserCount = 8;

    public async Task<List<ApplicationUser>> SeedAsync()
    {
        var definitions = CreateDefinitions();
        var users = new List<ApplicationUser>(definitions.Count);
        var createdCount = 0;

        foreach (var definition in definitions)
        {
            var normalizedEmail = userManager.NormalizeEmail(definition.Email);
            var user = await userManager.Users.SingleOrDefaultAsync(candidate =>
                candidate.NormalizedEmail == normalizedEmail);

            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = definition.UserName,
                    Email = definition.Email,
                    EmailConfirmed = definition.EmailConfirmed,
                    PhoneNumber = definition.PhoneNumber,
                    PhoneNumberConfirmed = definition.PhoneNumberConfirmed,
                    LastLoginAt = definition.LastLoginAt,
                };

                EnsureSucceeded(
                    await userManager.CreateAsync(user, DemoPassword),
                    $"create demo user '{definition.Email}'");
                createdCount++;
            }
            else
            {
                if (!string.Equals(user.UserName, definition.UserName, StringComparison.Ordinal))
                {
                    EnsureSucceeded(
                        await userManager.SetUserNameAsync(user, definition.UserName),
                        $"set the username for demo user '{definition.Email}'");
                }

                if (!await userManager.CheckPasswordAsync(user, DemoPassword))
                {
                    var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                    EnsureSucceeded(
                        await userManager.ResetPasswordAsync(user, resetToken, DemoPassword),
                        $"reset the password for demo user '{definition.Email}'");
                }
            }

            await EnsureRoleClaimAsync(user, definition.Role);
            users.Add(user);
        }

        logger.LogInformation(
            "Ensured {UserCount} demo users; created {CreatedUserCount}.",
            users.Count,
            createdCount);

        return users;
    }

    private static List<UserDefinition> CreateDefinitions()
    {
        var now = DateTimeOffset.UtcNow;
        var definitions = new List<UserDefinition>
        {
            new(
                "NaidenAdministrator",
                AdministratorEmail,
                UserRoles.Administrator,
                true,
                "+359888000001",
                true,
                now.AddDays(-1)),
            new(
                "NaidenClient",
                ClientEmail,
                UserRoles.Client,
                true,
                "+359888000002",
                true,
                now.AddHours(-4)),
        };

        for (var index = 1; index <= GenericUserCount; index++)
        {
            definitions.Add(
                new UserDefinition(
                    $"user{index:0000}",
                    $"user{index:0000}@{DemoEmailDomain}",
                    UserRoles.Client,
                    index % 2 == 0,
                    $"+359888{index:000000}",
                    index % 3 == 0,
                    index % 5 == 0 ? now.AddDays(-(index % 30)) : null));
        }

        return definitions;
    }

    private async Task EnsureRoleClaimAsync(ApplicationUser user, string role)
    {
        var claims = await userManager.GetClaimsAsync(user);
        var roleClaims = claims
            .Where(claim => claim.Type == UserClaimTypes.UserType)
            .ToList();

        if (roleClaims.Count == 1 && roleClaims[0].Value == role)
        {
            return;
        }

        if (roleClaims.Count > 0)
        {
            EnsureSucceeded(
                await userManager.RemoveClaimsAsync(user, roleClaims),
                $"remove existing role claims for demo user '{user.Email}'");
        }

        EnsureSucceeded(
            await userManager.AddClaimAsync(
                user,
                new Claim(UserClaimTypes.UserType, role)),
            $"assign role '{role}' to demo user '{user.Email}'");
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(", ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"Failed to {operation}: {errors}");
    }

    private sealed record UserDefinition(
        string UserName,
        string Email,
        string Role,
        bool EmailConfirmed,
        string? PhoneNumber,
        bool PhoneNumberConfirmed,
        DateTimeOffset? LastLoginAt);
}
