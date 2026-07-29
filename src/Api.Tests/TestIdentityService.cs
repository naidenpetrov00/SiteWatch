using Application.Identity.Commands;
using Application.Identity.Queries.DashboardUsers;
using Application.Identity.Queries.Users;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Application.SeedWork.Security;
using Domain.Entities;

namespace Api.Tests;

public sealed class TestIdentityService : IIdentityService
{
    private readonly Dictionary<string, string> _roles = new(StringComparer.Ordinal);

    public string? LastAssignedUserId { get; private set; }
    public string? LastAssignedRole { get; private set; }

    public void SetRole(string userId, string role) => _roles[userId] = role;

    public Task<string?> GetUserNameAsync(string userId) => Task.FromResult<string?>(userId);

    public Task<PagedResult<DashboardUserDto>> GetUsersAsync(
        DashboardUsersQuery query,
        CancellationToken cancellationToken
    ) => Task.FromResult(new PagedResult<DashboardUserDto>([], 0, 0));

    public Task<ApplicationUser?> FindUserByEmailAsync(string email)
    {
        var userId = email.StartsWith("admin", StringComparison.OrdinalIgnoreCase)
            ? "administrator-user"
            : email.StartsWith("worker", StringComparison.OrdinalIgnoreCase)
                ? "worker-user"
                : "client-user";
        var role = userId switch
        {
            "administrator-user" => UserRoles.Administrator,
            "worker-user" => UserRoles.Worker,
            _ => UserRoles.Client,
        };
        SetRole(userId, role);

        return Task.FromResult<ApplicationUser?>(new ApplicationUser
        {
            Id = userId,
            Email = email,
            UserName = userId,
        });
    }

    public Task<bool> IsInRoleAsync(string userId, string role) =>
        Task.FromResult(_roles.TryGetValue(userId, out var currentRole) && currentRole == role);

    public Task<bool> AuthorizeAsync(string userId, string policyName) => Task.FromResult(false);

    public Task<IdentityResultModel> AssignRoleAsync(string userId, string role)
    {
        if (!UserRoles.IsSupported(role))
        {
            return Task.FromResult<IdentityResultModel>(new IdentityResultOnly
            {
                Result = Result.Failure([IdentityResultErrors.UnsupportedRole]),
            });
        }

        LastAssignedUserId = userId;
        LastAssignedRole = role;
        SetRole(userId, role);
        return Task.FromResult<IdentityResultModel>(new IdentityResultOnly
        {
            Result = Result.Success(),
        });
    }

    public Task<IdentityResultModel> CheckAdministratorPasswordAsync(
        ApplicationUser user,
        string password
    )
    {
        if (!_roles.TryGetValue(user.Id, out var role) || role != UserRoles.Administrator)
        {
            return Task.FromResult<IdentityResultModel>(InvalidCredentials());
        }

        return Task.FromResult<IdentityResultModel>(Authenticated(user, role));
    }

    public Task<IdentityResultModel> CreateUserAsync(
        string userName,
        string email,
        string password
    ) => Task.FromResult<IdentityResultModel>(new IdentityResultWithEmail
    {
        Result = Result.Success(),
        Email = email,
    });

    public Task<Result> DeleteUserAsync(string userId) => Task.FromResult(Result.Success());

    public Task<IdentityResultModel> CheckPasswordAsync(ApplicationUser user, string password)
    {
        var role = _roles.GetValueOrDefault(user.Id, UserRoles.Client);
        return Task.FromResult<IdentityResultModel>(Authenticated(user, role));
    }

    public Task UpdateLastLoginAtAsync(ApplicationUser user) => Task.CompletedTask;

    public Task<IdentityResultModel> ConfirmEmailAsync(ApplicationUser user, string token) =>
        CheckPasswordAsync(user, token);

    public string GenerateVerificationToken() => "123456";

    public Task<bool> IsVerifiedEmailAsync(string email) => Task.FromResult(true);

    public Task<IdentityResultModel> ResetPasswordAsync(
        ApplicationUser user,
        string token,
        string newPassword
    ) => Task.FromResult<IdentityResultModel>(new IdentityResultOnly
    {
        Result = Result.Success(),
    });

    private static IdentityResultOnly InvalidCredentials() => new()
    {
        Result = Result.Failure([IdentityResultErrors.InvalidCredentials]),
    };

    private static IdentityResultWithUserToken Authenticated(ApplicationUser user, string role) =>
        new()
        {
            Result = Result.Success(),
            Token = "synthetic-test-token",
            User = new UserInfoDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Roles = [role],
            },
        };
}
