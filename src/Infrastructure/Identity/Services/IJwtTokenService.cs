using Application.SeedWork.Models;

namespace Infrastructure.Identity.Services;

public interface IJwtTokenService
{
    string GenerateToken(ApplicationUser user);
}
