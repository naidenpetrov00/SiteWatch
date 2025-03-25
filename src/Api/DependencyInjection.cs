using System.Text;
using Ardalis.GuardClauses;
using Infrastructure.Data.Options;
using Infrastructure.SeedWork.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Api;

public static class DependencyInjection
{
    public static void AddApiServices(
        this WebApplicationBuilder builder,
        IConfiguration configuration
    )
    {
        builder.Services.AddOpenApi();

        var options = Guard.Against.Null(configuration.GetOptions<JwtOptions>());
        var issuer = Guard.Against.Null(options.Issuer);
        var audience = Guard.Against.Null(options.Audience);
        var key = Guard.Against.Null(options.Key);

        builder
            .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                };
            });

        builder.Services.AddAuthorization();
    }
}
