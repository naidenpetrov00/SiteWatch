using Api;
using Api.SeedWork.Extensions;
using Application;
using Infrastructure;
using Infrastructure.SeedWork.Extension;

var builder = WebApplication.CreateBuilder(args);

builder.AddApiServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.InitializeDatabaseAsync();
    app.MapOpenApi();
    app.MapScalarApiReferenceWithOptions();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

app.Run();
