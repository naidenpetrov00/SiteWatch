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

if (app.Environment.IsDevelopment())
{
    await app.InitializeDatabaseAsync();
    builder.WebHost.UseUrls("http://0.0.0.0:5293");
    app.MapOpenApi();
    app.MapScalarApiReferenceWithOptions();
    app.UseCors("DevCors");
    app.UseRequestResponseLogging();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapEndpoints();
app.MapGet(
        "/auth-test",
        (HttpContext ctx) =>
        {
            var user = ctx.User;
            return Results.Ok(
                new
                {
                    IsAuthenticated = user.Identity?.IsAuthenticated ?? false,
                    Name = user.Identity?.Name,
                    Claims = user.Claims.Select(c => new { c.Type, c.Value }),
                }
            );
        }
    )
    .RequireAuthorization();

app.Run();
