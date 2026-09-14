using System.Reflection;
using System.Text;
using Api.Services;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Ardalis.GuardClauses;
using Infrastructure.SeedWork.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
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
        builder.Services.AddDataProtection();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IUser, CurrentUser>();
        builder.Services.AddSingleton<
            IInvoiceFileAccessTicketService,
            InvoiceFileAccessTicketService>();
        builder.Services.AddSingleton<
            IIssueAttachmentAccessTicketService,
            IssueAttachmentAccessTicketService>();
        builder.Services.AddAutoMapper(cfg => { }, Assembly.GetExecutingAssembly());
        builder.Services.AddCors(opt =>
        {
            opt.AddPolicy(
                "DevCors",
                policy =>
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .SetPreflightMaxAge(TimeSpan.FromHours(1));
                }
            );
        });
        var options = Guard.Against.Null(configuration.GetOptions<JwtOptions>());
        var issuer = Guard.Against.Null(options.Issuer);
        var audience = Guard.Against.Null(options.Audience);
        var key = Guard.Against.Null(options.Key);

        builder
            .Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    RoleClaimType = UserClaimTypes.UserType,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                };
            });
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(
                AuthorizationPolicies.Administrator,
                policy => policy.RequireAuthenticatedUser().RequireRole(UserRoles.Administrator)
            );
            options.AddPolicy(
                AuthorizationPolicies.AdministratorOrWorker,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireRole(UserRoles.Administrator, UserRoles.Worker)
            );
        });
    }
}
