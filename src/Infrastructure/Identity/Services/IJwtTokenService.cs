using Application.SeedWork.Models;
using Domain.Entities;

namespace Infrastructure.Identity.Services;

public interface IJwtTokenService
{
    string GenerateToken(ApplicationUser user);
}
