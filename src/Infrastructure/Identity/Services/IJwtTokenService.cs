using Domain.Entities;

namespace Infrastructure.Identity.Services;

public sealed record GeneratedJwtToken(string Token, IReadOnlyList<string> Roles);

public interface IJwtTokenService
{
    Task<GeneratedJwtToken> GenerateTokenAsync(ApplicationUser user);
}
