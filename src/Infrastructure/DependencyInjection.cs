using System.Reflection;
using Application.SeedWork.Interfaces;
using Ardalis.GuardClauses;
using Azure.Storage.Blobs;
using Domain.Entities;
using Infrastructure.Cameras.Services;
using Infrastructure.Data;
using Infrastructure.Email;
using Infrastructure.Identity.Services;
using Infrastructure.SeedWork.Options;
using Infrastructure.Sites.Services;
using Infrastructure.Storage;
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
        var mssqlConnectionString = Guard.Against.NullOrEmpty(
            configuration.GetOptions<MssqlOptions>().ConnectionStringDockerDb,
            "Connection String for docker composed not found!"
        );
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(mssqlConnectionString)
        );

        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IBlobService, BlobService>();
        var blobStorageConnectionString = Guard.Against.NullOrEmpty(
            configuration.GetOptions<BlobStorageOptions>().ConnectionString);
        services.AddSingleton<BlobServiceClient>(_ =>
            new BlobServiceClient(blobStorageConnectionString));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>()
        );
        services.AddScoped<ApplicationDbContextInitialiser>();
        services.AddScoped<IEmailService, EmailService>();

        services.AddTransient<IIdentityService, IdentityService>();
        services.AddTransient<ISiteService, SiteService>();
        services.AddTransient<ICameraService, CameraService>();

        services
            .AddIdentity<ApplicationUser, IdentityRole>(options => { options.User.RequireUniqueEmail = true; })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddSignInManager();

        return services;
    }
}