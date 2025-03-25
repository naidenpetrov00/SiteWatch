using Application.SeedWork.Interfaces;

namespace Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(IUser user);
}
