using Api;
using Api.SeedWork.Extensions;
using Application;
using DotNetEnv;
using Infrastructure;
using Infrastructure.SeedWork.Extension;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.AddApiServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.InitializeDatabaseAsync();
    builder.WebHost.UseUrls("http://0.0.0.0:5293");
    app.MapOpenApi();
    app.MapScalarApiReferenceWithOptions();
    app.UseCors("DevCors");
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

app.Run();
