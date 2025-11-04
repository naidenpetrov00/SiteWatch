using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MyApp.Api.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestResponseLoggingMiddleware> logger
        )
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            string requestBody = await ReadRequestBody(context.Request);
            _logger.LogWarning(
                "Incoming request {Method} {Path} | Body: {Body}",
                context.Request.Method,
                context.Request.Path,
                string.IsNullOrWhiteSpace(requestBody) ? "<empty>" : requestBody
            );

            var originalBodyStream = context.Response.Body;
            await using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            stopwatch.Stop();

            string responseBodyText = await ReadResponseBody(context.Response);
            _logger.LogWarning(
                "Response {StatusCode} in {Elapsed} ms | Body: {Body}",
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                string.IsNullOrWhiteSpace(responseBodyText) ? "<empty>" : responseBodyText
            );

            await responseBody.CopyToAsync(originalBodyStream);
        }

        private async Task<string> ReadRequestBody(HttpRequest request)
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            string body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return body;
        }

        private async Task<string> ReadResponseBody(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            string body = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);
            return body;
        }
    }

    public static class RequestResponseLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestResponseLogging(
            this IApplicationBuilder builder
        )
        {
            return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
    }
}
