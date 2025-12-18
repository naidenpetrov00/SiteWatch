using Api;
using Api.SeedWork.Extensions;
using Application;
using DotNetEnv;
using Infrastructure;
using Infrastructure.SeedWork.Extension;
using MyApp.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.AddApiServices(builder.Configuration);
builder.Services.AddApplicationServices();

var app = builder.Build();
app.UseCustomMiddleware();

if (app.Environment.IsDevelopment())
{
    await app.InitializeDatabaseAsync();
    builder.WebHost.UseUrls("http://0.0.0.0:5293");
    app.MapOpenApi();
    app.MapScalarApiReferenceWithOptions();
    app.UseCors("DevCors");
    // app.UseRequestResponseLogging();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapEndpoints();

app.Run();