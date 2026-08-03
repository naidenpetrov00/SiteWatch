using System.Reflection;
using Application.SeedWork.Interfaces;
using Ardalis.GuardClauses;
using Azure.Storage.Blobs;
using Domain.Entities;
using Infrastructure.Cameras.Services;
using Infrastructure.Data;
using Infrastructure.Email;
using Infrastructure.Invoices.Services;
using Infrastructure.Identity.Services;
using Infrastructure.Persons.Services;
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
        var blobStorageOptions = configuration.GetOptions<BlobStorageOptions>();
        var blobStorageConnectionString = Guard.Against.NullOrEmpty(blobStorageOptions.ConnectionString);
        services.AddSingleton<BlobServiceClient>(_ =>
            new BlobServiceClient(
                blobStorageConnectionString,
                CreateBlobClientOptions(blobStorageOptions.ServiceVersion)));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>()
        );
        services.AddScoped<IImagesService, ImagesService>();
        services.AddScoped<IFilesService, FilesService>();
        services.AddScoped<IVideosService, VideosService>();
        services.AddScoped<ApplicationDbContextInitialiser>();
        services.AddScoped<BlobInitializer>();
        services.AddScoped<IBlobService, BlobImagesService>();
        services.AddScoped<IFilesBlobService, BlobFilesService>();
        services.AddScoped<IVideosBlobService, BlobVideosService>();
        services.AddScoped<IInvoiceBlobService, BlobInvoiceService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ISiteService, SiteService>();
        services.AddScoped<ICameraService, CameraService>();
        services.AddScoped<IPersonService, PersonService>();

        services
            .AddIdentity<ApplicationUser, IdentityRole>(options => { options.User.RequireUniqueEmail = true; })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddSignInManager();

        return services;
    }

    private static BlobClientOptions CreateBlobClientOptions(string? configuredServiceVersion)
    {
        if (string.IsNullOrWhiteSpace(configuredServiceVersion))
        {
            return new BlobClientOptions();
        }

        var enumName = $"V{configuredServiceVersion.Replace('-', '_')}";
        if (!Enum.TryParse<BlobClientOptions.ServiceVersion>(enumName, ignoreCase: true, out var serviceVersion))
        {
            throw new InvalidOperationException(
                $"Unsupported BlobStorage service version '{configuredServiceVersion}'.");
        }

        return new BlobClientOptions(serviceVersion);
    }
}
