using System.Reflection;
using System.Runtime.CompilerServices;

namespace Api.SeedWork.Extensions;

internal static class WebApplicationExtensions
{
    internal static RouteGroupBuilder MapGroupCustom(
        this WebApplication app,
        [CallerFilePath] string callerFilePath = "",
        string? customGroupName = null
    )
    {
        string groupName =
            customGroupName ?? Path.GetFileNameWithoutExtension(callerFilePath).ToLower();
        return app.MapGroup(groupName);
    }

    internal static WebApplication MapEndpoints(this WebApplication app)
    {
        var endpointGroupType = typeof(EndpointGroupBase);

        var assembly = Assembly.GetExecutingAssembly();

        var endpointGroupTypes = assembly
            .GetExportedTypes()
            .Where(t => t.IsSubclassOf(endpointGroupType));

        foreach (var type in endpointGroupTypes)
        {
            if (Activator.CreateInstance(type) is EndpointGroupBase instance)
            {
                instance.Map(app);
            }
        }

        return app;
    }
}
