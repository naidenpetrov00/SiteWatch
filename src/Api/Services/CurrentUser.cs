using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.SeedWork.Interfaces;

namespace Api.Services;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : IUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public string? Id =>
        Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? Principal?.FindFirstValue(JwtRegisteredClaimNames.NameId)
        ?? Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);

    public string? Email =>
        Principal?.FindFirstValue(ClaimTypes.Email)
        ?? Principal?.FindFirstValue(JwtRegisteredClaimNames.Email);
}
