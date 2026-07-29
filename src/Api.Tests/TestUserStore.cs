using System.Security.Claims;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Api.Tests;

internal sealed class TestUserStore
    : IUserPasswordStore<ApplicationUser>,
        IUserClaimStore<ApplicationUser>
{
    private readonly Dictionary<string, ApplicationUser> _users = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<Claim>> _claims = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string?> _passwordHashes = new(StringComparer.Ordinal);

    public IReadOnlyList<Claim> ClaimsFor(ApplicationUser user) =>
        _claims.GetValueOrDefault(user.Id, []);

    public void Dispose() { }

    public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        Task.FromResult(user.Id);

    public Task<string?> GetUserNameAsync(
        ApplicationUser user,
        CancellationToken cancellationToken
    ) => Task.FromResult(user.UserName);

    public Task SetUserNameAsync(
        ApplicationUser user,
        string? userName,
        CancellationToken cancellationToken
    )
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(
        ApplicationUser user,
        CancellationToken cancellationToken
    ) => Task.FromResult(user.NormalizedUserName);

    public Task SetNormalizedUserNameAsync(
        ApplicationUser user,
        string? normalizedName,
        CancellationToken cancellationToken
    )
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public Task<IdentityResult> CreateAsync(
        ApplicationUser user,
        CancellationToken cancellationToken
    )
    {
        _users[user.Id] = user;
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> UpdateAsync(
        ApplicationUser user,
        CancellationToken cancellationToken
    )
    {
        _users[user.Id] = user;
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> DeleteAsync(
        ApplicationUser user,
        CancellationToken cancellationToken
    )
    {
        _users.Remove(user.Id);
        _claims.Remove(user.Id);
        _passwordHashes.Remove(user.Id);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<ApplicationUser?> FindByIdAsync(
        string userId,
        CancellationToken cancellationToken
    ) => Task.FromResult(_users.GetValueOrDefault(userId));

    public Task<ApplicationUser?> FindByNameAsync(
        string normalizedUserName,
        CancellationToken cancellationToken
    ) => Task.FromResult(_users.Values.FirstOrDefault(user =>
        user.NormalizedUserName == normalizedUserName
    ));

    public Task SetPasswordHashAsync(
        ApplicationUser user,
        string? passwordHash,
        CancellationToken cancellationToken
    )
    {
        _passwordHashes[user.Id] = passwordHash;
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(
        ApplicationUser user,
        CancellationToken cancellationToken
    ) => Task.FromResult(_passwordHashes.GetValueOrDefault(user.Id));

    public Task<bool> HasPasswordAsync(
        ApplicationUser user,
        CancellationToken cancellationToken
    ) => Task.FromResult(_passwordHashes.GetValueOrDefault(user.Id) is not null);

    public Task<IList<Claim>> GetClaimsAsync(
        ApplicationUser user,
        CancellationToken cancellationToken
    ) => Task.FromResult<IList<Claim>>(_claims.GetValueOrDefault(user.Id, []).ToList());

    public Task AddClaimsAsync(
        ApplicationUser user,
        IEnumerable<Claim> claims,
        CancellationToken cancellationToken
    )
    {
        if (!_claims.TryGetValue(user.Id, out var userClaims))
        {
            userClaims = [];
            _claims[user.Id] = userClaims;
        }

        userClaims.AddRange(claims);
        return Task.CompletedTask;
    }

    public Task ReplaceClaimAsync(
        ApplicationUser user,
        Claim claim,
        Claim newClaim,
        CancellationToken cancellationToken
    )
    {
        var userClaims = _claims.GetValueOrDefault(user.Id, []);
        var index = userClaims.FindIndex(item => item.Type == claim.Type && item.Value == claim.Value);
        if (index >= 0) userClaims[index] = newClaim;
        return Task.CompletedTask;
    }

    public Task RemoveClaimsAsync(
        ApplicationUser user,
        IEnumerable<Claim> claims,
        CancellationToken cancellationToken
    )
    {
        var userClaims = _claims.GetValueOrDefault(user.Id, []);
        foreach (var claim in claims)
        {
            userClaims.RemoveAll(item => item.Type == claim.Type && item.Value == claim.Value);
        }
        return Task.CompletedTask;
    }

    public Task<IList<ApplicationUser>> GetUsersForClaimAsync(
        Claim claim,
        CancellationToken cancellationToken
    ) => Task.FromResult<IList<ApplicationUser>>(
        _users.Values
            .Where(user => ClaimsFor(user).Any(item => item.Type == claim.Type && item.Value == claim.Value))
            .ToList()
    );
}
