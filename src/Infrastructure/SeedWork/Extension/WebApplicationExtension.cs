
using Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.SeedWork.Extension;
public static class WebApplicationExtension
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initializer =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initializer.InitializeDatabaseAsync();
    }
}
