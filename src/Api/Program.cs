using Api;
using Api.Logging;
using Api.SeedWork.Extensions;
using Application;
using DotNetEnv;
using Infrastructure;
using Infrastructure.SeedWork.Extension;

var builder = WebApplication.CreateBuilder(args);

builder.Services.FilterRetailerPriceCollectionPollingFromConsole();

Env.Load();

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.AddApiServices(builder.Configuration);
builder.Services.AddApplicationServices();

if (builder.Environment.IsDevelopment())
{
    builder.WebHost.UseUrls("http://0.0.0.0:5293");
}

var app = builder.Build();

if (args.Contains("--migrate-database", StringComparer.Ordinal))
{
    await app.ApplyDatabaseMigrationsAsync();
    return;
}

if (!app.Environment.IsDevelopment())
{
    app.UseForwardedHeaders();
}

app.UseCustomMiddleware();

if (app.Environment.IsDevelopment())
{
    await app.InitializeDatabaseAsync();
    app.MapOpenApi();
    app.MapScalarApiReferenceWithOptions();
    app.UseCors(DependencyInjection.DevelopmentCorsPolicy);
    // app.UseRequestResponseLogging();
}
else
{
    app.UseCors(DependencyInjection.ProductionCorsPolicy);
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/healthz");
app.MapEndpoints();

app.Run();

public partial class Program;
