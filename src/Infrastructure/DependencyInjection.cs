using System.Reflection;
using Application.SeedWork.Interfaces;
using Ardalis.GuardClauses;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Data.Options;
using Infrastructure.Email;
using Infrastructure.Identity.Services;
using Infrastructure.SeedWork.Options;
using Infrastructure.Sites.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
        );
        var mssqlConnectionString = Guard.Against.Null(
            configuration.GetOptions<MssqlOptions>().ConnectionStringDockerDb,
            "Connection String for docker composed not found!"
        );

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(mssqlConnectionString)
        );
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>()
        );
        services.AddScoped<ApplicationDbContextInitialiser>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddTransient<IIdentityService, IdentityService>();
        services.AddTransient<ISiteService, SiteService>();

        services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }
}
