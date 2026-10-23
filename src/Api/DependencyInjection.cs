using System.Reflection;
using System.Text;
using Api.Services;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Ardalis.GuardClauses;
using Infrastructure.SeedWork.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;

namespace Api;

public static class DependencyInjection
{
    public const string DevelopmentCorsPolicy = "DevCors";
    public const string ProductionCorsPolicy = "ProductionCors";

    public static void AddApiServices(
        this WebApplicationBuilder builder,
        IConfiguration configuration
    )
    {
        builder.Services.AddOpenApi();
        builder.Services.AddHealthChecks();

        var dataProtection = builder.Services
            .AddDataProtection()
            .SetApplicationName("SiteWatch");
        var dataProtectionKeysPath = configuration["DataProtection:KeysPath"];
        if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath))
        {
            dataProtection.PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));
        }
        else if (!builder.Environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "DataProtection:KeysPath must be configured outside Development."
            );
        }

        if (!builder.Environment.IsDevelopment())
        {
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

                // The API port is private to Docker. Caddy is its only ingress proxy.
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
            });
        }

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IUser, CurrentUser>();
        builder.Services.AddHostedService<RetailerPriceCollectionHostedService>();
        builder.Services.AddSingleton<
            IInvoiceFileAccessTicketService,
            InvoiceFileAccessTicketService>();
        builder.Services.AddSingleton<
            IIssueAttachmentAccessTicketService,
            IssueAttachmentAccessTicketService>();
        builder.Services.AddSingleton<
            IProposalPdfAccessTicketService,
            ProposalPdfAccessTicketService>();
        builder.Services.AddSingleton<
            IClientProposalPdfAccessTicketService,
            ClientProposalPdfAccessTicketService>();
        builder.Services.AddAutoMapper(cfg => { }, Assembly.GetExecutingAssembly());
        builder.Services.AddCors(opt =>
        {
            opt.AddPolicy(
                DevelopmentCorsPolicy,
                policy =>
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .SetPreflightMaxAge(TimeSpan.FromHours(1));
                }
            );

            if (!builder.Environment.IsDevelopment())
            {
                var allowedOrigins = configuration
                    .GetSection("Cors:AllowedOrigins")
                    .Get<string[]>() ?? [];
                if (allowedOrigins.Length == 0)
                {
                    throw new InvalidOperationException(
                        "Cors:AllowedOrigins must contain at least one production origin."
                    );
                }

                opt.AddPolicy(
                    ProductionCorsPolicy,
                    policy => policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .SetPreflightMaxAge(TimeSpan.FromHours(1))
                );
            }
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
