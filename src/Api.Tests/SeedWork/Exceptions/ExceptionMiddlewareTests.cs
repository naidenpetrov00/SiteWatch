using Api.SeedWork.Exceptions;
using Application.Cameras;
using Application.SeedWork.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text;

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
            Task.FromException((Exception)Activator.CreateInstance(exceptionType)!),
            NullLogger<ExceptionMiddleware>.Instance);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        Assert.Equal(expectedStatus, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_returns_field_details_for_validation_failures()
    {
        var middleware = new ExceptionMiddleware(_ => Task.FromException(new ValidationException(
            [new ValidationFailure("invoiceNumber", "Invoice number is required.", "NotEmptyValidator")])),
            NullLogger<ExceptionMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        var payload = await new StreamReader(context.Response.Body, Encoding.UTF8).ReadToEndAsync();
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Contains("validation_error", payload);
        Assert.Contains("invoiceNumber", payload);
        Assert.Contains("Invoice number is required.", payload);
    }

    [Fact]
    public async Task InvokeAsync_returns_a_safe_bad_gateway_problem_for_camera_communication_failures()
    {
        var middleware = new ExceptionMiddleware(
            _ => Task.FromException(new CameraCommunicationException()),
            NullLogger<ExceptionMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Request.Path = "/cameras/camera-42/snapshot";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        var payload = await new StreamReader(context.Response.Body, Encoding.UTF8).ReadToEndAsync();
        Assert.Equal(StatusCodes.Status502BadGateway, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);
        Assert.Contains("Camera unavailable", payload);
        Assert.Contains("The camera did not accept the request.", payload);
        Assert.DoesNotContain("Unable to communicate", payload);
        Assert.Contains("/cameras/camera-42/snapshot", payload);
    }
}
