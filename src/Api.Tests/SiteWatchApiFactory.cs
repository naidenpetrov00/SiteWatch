using Application.SeedWork.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Api.Tests;

public sealed class SiteWatchApiFactory : WebApplicationFactory<Program>
{
    public const string Issuer = "https://sitewatch.test";
    public const string Audience = "sitewatch-tests";
    public const string SigningKey =
        "sitewatch-tests-only-signing-key-with-at-least-sixty-four-characters-123456";

    public TestIdentityService IdentityService { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Mssql:ConnectionStringDockerDb"] =
                    "Server=localhost;Database=unused;User Id=unused;Password=unused;TrustServerCertificate=True",
                ["BlobStorage:ConnectionString"] = "UseDevelopmentStorage=true",
                ["Jwt:Key"] = SigningKey,
                ["Jwt:Issuer"] = Issuer,
                ["Jwt:Audience"] = Audience,
                ["Jwt:ExpireDays"] = "1",
            });
        });
        builder.ConfigureServices(services =>
        {
            services.AddDataProtection().UseEphemeralDataProtectionProvider();
            services.RemoveAll<IIdentityService>();
            services.AddSingleton<IIdentityService>(IdentityService);
        });
    }
}
