namespace Infrastructure;

using System.Reflection;
using Application.SeedWork.Interfaces;
using Ardalis.GuardClauses;
using Infrastructure.Data;
using Infrastructure.Data.Options;
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
#if UseDocker
        var mssqlConnectionString = configuration
            .GetOptions<MssqlOptions>("MSSQL")
            .ConnectionStringDockerCompose;
        Guard.Against.Null(
            mssqlConnectionString,
            message: "Connection String for docker composed not found!"
        );
#else
        var mssqlConnectionString = configuration
            .GetOptions<MssqlOptions>("MSSQL")
            .ConnectionStringDotNetBuildWithDockerDb;
        Guard.Against.Null(
            mssqlConnectionString,
            message: "Connection String for docker db only not found!"
        );
#endif

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(mssqlConnectionString)
        );
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>()
        );
        services.AddScoped<ApplicationDbContextInitialiser>();

        return services;
    }
}
