using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ardalis.GuardClauses;
using Application.SeedWork.Security;
using Domain.Entities;
using Infrastructure.SeedWork.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Identity.Services;

public class JwtTokenService(
    IConfiguration configuration,
    UserManager<ApplicationUser> userManager
) : IJwtTokenService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<string> GenerateTokenAsync(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.NameId, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var userClaims = await _userManager.GetClaimsAsync(user);
        var userTypeClaim = userClaims.FirstOrDefault(c => c.Type == UserClaimTypes.UserType);
        if (userTypeClaim is not null)
        {
            claims.Add(new Claim(UserClaimTypes.UserType, userTypeClaim.Value));
        }

        var options = Guard.Against.Null(_configuration.GetOptions<JwtOptions>());
        var issuerFromConfiguration = Guard.Against.Null(options.Issuer);
        var audienceFromConfiguration = Guard.Against.Null(options.Audience);
        var keyFromConfiguration = Guard.Against.Null(options.Key);
        var expiresFromConfiguration = Guard.Against.Null(options.ExpireDays);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyFromConfiguration));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuerFromConfiguration,
            audience: audienceFromConfiguration,
            // expires: DateTime.Now.AddDays(int.Parse(expiresFromConfiguration)),
            expires: DateTime.Now.AddMinutes(1),
            claims: claims,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
