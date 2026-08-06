using Api.SeedWork.Exceptions;
using Application.SeedWork.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Api.Tests.SeedWork.Exceptions;

public sealed class ExceptionMiddlewareTests
{
    [Theory]
    [InlineData(typeof(UnauthorizedAccessException), StatusCodes.Status401Unauthorized)]
    [InlineData(typeof(ForbiddenAccessException), StatusCodes.Status403Forbidden)]
    public async Task InvokeAsync_maps_access_exceptions_to_the_expected_status(
        Type exceptionType,
        int expectedStatus)
    {
        var middleware = new ExceptionMiddleware(_ =>
            Task.FromException((Exception)Activator.CreateInstance(exceptionType)!));
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        Assert.Equal(expectedStatus, context.Response.StatusCode);
    }
}
