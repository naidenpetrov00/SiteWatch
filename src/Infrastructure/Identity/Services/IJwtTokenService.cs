using Domain.Entities;

namespace Infrastructure.Identity.Services;

public interface IJwtTokenService
{
    Task<string> GenerateTokenAsync(ApplicationUser user);
}
