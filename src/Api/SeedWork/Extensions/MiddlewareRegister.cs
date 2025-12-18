using Api.SeedWork.Exceptions;

namespace Api.SeedWork.Extensions;

internal static class MiddlewareRegister
{
    internal static WebApplication UseCustomMiddleware(this WebApplication app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
        return app;
    }
}