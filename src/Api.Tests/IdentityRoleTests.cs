using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Identity.Commands;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Application.SeedWork.Security;
using Domain.Entities;
using Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Api.Tests;

public sealed class IdentityRoleTests
{
    [Fact]
    public void UserRoles_contains_exactly_the_supported_roles()
    {
        Assert.Equal(
            [UserRoles.Administrator, UserRoles.Client, UserRoles.Worker],
            UserRoles.All.OrderBy(role => role)
        );
        Assert.Equal(
            UserRoles.Administrator + "," + UserRoles.Worker,
            UserRoleGroups.AdministratorOrWorker
        );
    }

    [Fact]
    public async Task CreateUser_assigns_client_role()
    {
        var (store, userManager) = CreateUserManager();
        var service = CreateIdentityUserService(userManager);

        var result = await service.CreateUserAsync("new-user", "new@example.test", "Password1!");
        var user = await userManager.FindByNameAsync("new-user");

        Assert.True(result.Result.Succeeded);
        Assert.NotNull(user);
        Assert.Contains(
            store.ClaimsFor(user),
            claim => claim.Type == UserClaimTypes.UserType && claim.Value == UserRoles.Client
        );
    }

    [Theory]
    [InlineData(UserRoles.Administrator)]
    [InlineData(UserRoles.Client)]
    [InlineData(UserRoles.Worker)]
    public async Task AssignRole_replaces_the_existing_role_and_is_idempotent(string role)
    {
        var (store, userManager) = CreateUserManager();
        var user = new ApplicationUser { Id = "role-user", UserName = "role-user" };
        await userManager.CreateAsync(user);
        await userManager.AddClaimAsync(
            user,
            new Claim(UserClaimTypes.UserType, UserRoles.Client)
        );
        var service = CreateIdentityUserService(userManager);

        var firstResult = await service.AssignRoleAsync(user.Id, role);
        var secondResult = await service.AssignRoleAsync(user.Id, role);
        var roleClaims = store.ClaimsFor(user)
            .Where(claim => claim.Type == UserClaimTypes.UserType)
            .ToList();

        Assert.True(firstResult.Result.Succeeded);
        Assert.True(secondResult.Result.Succeeded);
        var claim = Assert.Single(roleClaims);
        Assert.Equal(role, claim.Value);
    }

    [Fact]
    public async Task AssignRole_rejects_unsupported_role()
    {
        var (_, userManager) = CreateUserManager();
        var service = CreateIdentityUserService(userManager);

        var result = await service.AssignRoleAsync("missing", "Unknown");

        Assert.False(result.Result.Succeeded);
        Assert.Contains(IdentityResultErrors.UnsupportedRole, result.Result.Errors);
    }

    [Fact]
    public async Task JwtToken_contains_the_custom_role_claim_and_exposes_roles()
    {
        var (_, userManager) = CreateUserManager();
        var user = new ApplicationUser
        {
            Id = "worker-user",
            UserName = "worker-user",
            Email = "worker@example.test",
        };
        await userManager.CreateAsync(user);
        await userManager.AddClaimAsync(
            user,
            new Claim(UserClaimTypes.UserType, UserRoles.Worker)
        );
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = SiteWatchApiFactory.SigningKey,
                ["Jwt:Issuer"] = SiteWatchApiFactory.Issuer,
                ["Jwt:Audience"] = SiteWatchApiFactory.Audience,
                ["Jwt:ExpireDays"] = "1",
            })
            .Build();
        var service = new JwtTokenService(configuration, userManager);

        var result = await service.GenerateTokenAsync(user);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);

        Assert.Equal([UserRoles.Worker], result.Roles);
        var roleClaim = Assert.Single(token.Claims, claim => claim.Type == UserClaimTypes.UserType);
        Assert.Equal(UserRoles.Worker, roleClaim.Value);
    }

    private static IdentityUserService CreateIdentityUserService(
        UserManager<ApplicationUser> userManager
    ) => new(userManager, new TestVerificationService(), new TestEmailService(), null!);

    private static (TestUserStore Store, UserManager<ApplicationUser> UserManager) CreateUserManager()
    {
        var store = new TestUserStore();
        var services = new ServiceCollection().BuildServiceProvider();
        var userManager = new UserManager<ApplicationUser>(
            store,
            Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(),
            [],
            [],
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            services,
            NullLogger<UserManager<ApplicationUser>>.Instance
        );
        return (store, userManager);
    }

    private sealed class TestVerificationService : IIdentityVerificationService
    {
        public string GenerateVerificationToken() => "123456";

        public Task<IdentityResultModel> ConfirmEmailAsync(
            ApplicationUser user,
            string emailToken
        ) => Task.FromResult<IdentityResultModel>(new IdentityResultOnly
        {
            Result = Result.Success(),
        });

        public Task<IdentityResultModel> ResetPasswordAsync(
            ApplicationUser user,
            string token,
            string newPassword
        ) => Task.FromResult<IdentityResultModel>(new IdentityResultOnly
        {
            Result = Result.Success(),
        });

        public Task<bool> IsVerifiedEmailAsync(string email) => Task.FromResult(true);
    }

    private sealed class TestEmailService : IEmailService
    {
        public Task SendVerifyEmailAsync(ApplicationUser user, string toEmail, string token) =>
            Task.CompletedTask;

        public Task SendPasswordResetEmailAsync(string toEmail, string token) =>
            Task.CompletedTask;
    }
}
