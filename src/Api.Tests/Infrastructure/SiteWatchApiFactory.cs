using System.Security.Claims;
using System.Text.Encodings.Web;
using Application.SeedWork.Security;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Api.Tests.Infrastructure;

public sealed class SiteWatchApiFactory : WebApplicationFactory<Program>
{
    private const string AuthenticationScheme = "SiteWatchTests";

    public IMediator Mediator { get; } = Substitute.For<IMediator>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Mssql:ConnectionStringDockerDb"] =
                    "Server=(localdb)\\mssqllocaldb;Database=SiteWatchApiTests;Trusted_Connection=True;",
                ["BlobStorage:ConnectionString"] = "UseDevelopmentStorage=true",
                ["Jwt:Issuer"] = "SiteWatchApiTests",
                ["Jwt:Audience"] = "SiteWatchApiTests",
                ["Jwt:Key"] = "site-watch-api-tests-only-signing-key-0000000000000000",
            }));

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IMediator>();
            services.AddSingleton(Mediator);
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = AuthenticationScheme;
                    options.DefaultChallengeScheme = AuthenticationScheme;
                    options.DefaultScheme = AuthenticationScheme;
                })
                .AddScheme<AuthenticationSchemeOptions, AdministratorAuthenticationHandler>(
                    AuthenticationScheme,
                    _ => { });
        });
    }

    public HttpClient CreateHttpsClient() => CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false,
    });

    private sealed class AdministratorAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            Claim[] claims =
            [
                new(ClaimTypes.NameIdentifier, "site-watch-api-test-administrator"),
                new(UserClaimTypes.UserType, UserRoles.Administrator),
            ];
            var identity = new ClaimsIdentity(
                claims,
                Scheme.Name,
                ClaimTypes.NameIdentifier,
                UserClaimTypes.UserType);
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
