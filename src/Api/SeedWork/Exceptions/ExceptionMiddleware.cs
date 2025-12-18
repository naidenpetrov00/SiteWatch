using System.Text.Json;
using Ardalis.GuardClauses;
using FluentValidation;

namespace Api.SeedWork.Exceptions;

internal sealed class ExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.Headers.ContentType = "application/json";

            var errors = ex.Errors.Select(e => new
            {
                field = e.PropertyName,
                message = e.ErrorMessage,
                code = e.ErrorCode
            });

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = "validation_error",
                details = errors
            }));
        }
        catch (NotFoundException ex)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "application/json";
            var payload = new
            {
                title = "Resource not found",
                detail = ex.Message,
                data = ex.Data,
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.Headers.Append("content-type", "application/json");

            var errorCode = ToUnderscoreCase(ex.GetType().Name.Replace("Exception", string.Empty));
            var json = JsonSerializer.Serialize(
                new { ErrorCode = errorCode, ErrorMessage = ex.Message }
            );
            await context.Response.WriteAsync(json);
        }
    }

    private static string ToUnderscoreCase(string value) =>
        string.Concat(
                (value ?? string.Empty).Select((x, i) =>
                    i > 0 && char.IsUpper(x) && !char.IsUpper(value![i - 1])
                        ? $"_{x}"
                        : x.ToString()
                )
            )
            .ToLower();
}