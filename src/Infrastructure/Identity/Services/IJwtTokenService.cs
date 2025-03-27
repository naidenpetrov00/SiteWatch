using Infrastructure.Identity;

namespace Application.SeedWork.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(ApplicationUser user);
}
