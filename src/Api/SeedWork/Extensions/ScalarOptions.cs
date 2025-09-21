using Scalar.AspNetCore;

namespace Api.SeedWork.Extensions;

internal static class ScalarApiReferenceExtensions
{
    internal static void MapScalarApiReferenceWithOptions(this WebApplication app)
    {
        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("SiteWatchApi")
                .WithTheme(ScalarTheme.Mars)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                .AddPreferredSecuritySchemes("Bearer");
        });
    }
}
