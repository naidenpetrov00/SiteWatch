namespace Infrastructure;

using System.Reflection;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Ardalis.GuardClauses;
using Infrastructure.Data;
using Infrastructure.Data.Options;
using Infrastructure.Identity.Services;
using Infrastructure.SeedWork.Options;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
