using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Interfaces;
using Application.SeedWork.Interfaces;
using Ardalis.GuardClauses;
using Infrastructure.Data.Options;
using Infrastructure.SeedWork.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

public class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    private readonly IConfiguration _configuration = configuration;

    public string GenerateToken(IUser user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            // Add more claims as needed (e.g., roles)
        };

        var options = Guard.Against.Null(_configuration.GetOptions<JwtOptions>());
        var issuerFromConfiguration = Guard.Against.Null(options.Issuer);
        var audienceFromConfiguration = Guard.Against.Null(options.Audience);
        var keyFromConfiguration = Guard.Against.Null(options.Key);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyFromConfiguration));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuerFromConfiguration,
            audience: audienceFromConfiguration,
            expires: DateTime.Now.AddDays(15),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
