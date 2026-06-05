using System.Reflection;
using Application.Invoices.Services;
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

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IIdentityAuthenticationService, IdentityAuthenticationService>();
        services.AddScoped<IIdentityVerificationService, IdentityVerificationService>();
        services.AddScoped<IIdentityUserService, IdentityUserService>();
        var blobStorageConnectionString = Guard.Against.NullOrEmpty(
            configuration.GetOptions<BlobStorageOptions>().ConnectionString);
        services.AddSingleton<BlobServiceClient>(_ =>
            new BlobServiceClient(blobStorageConnectionString));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>()
        );
        services.AddScoped<IImagesService, ImagesService>();
        services.AddScoped<IFilesService, FilesService>();
        services.AddScoped<IVideosService, VideosService>();
        services.AddScoped<ApplicationDbContextInitialiser>();
        services.AddScoped<IBlobService, BlobImagesService>();
        services.AddScoped<IFilesBlobService, BlobFilesService>();
        services.AddScoped<IVideosBlobService, BlobVideosService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ISiteService, SiteService>();
        services.AddScoped<ICameraService, CameraService>();
        services.AddScoped<IInvoiceFileStorage, LocalInvoiceFileStorage>();

        services
            .AddIdentity<ApplicationUser, IdentityRole>(options => { options.User.RequireUniqueEmail = true; })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddSignInManager();

        return services;
    }
}
