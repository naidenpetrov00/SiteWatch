namespace Api.SeedWork.EndpointFilters;

public class AuthorizationFilter : IEndpointFilter
{
    // The InvokeAsync method is called by the runtime when the endpoint is executed.
    // It takes the EndpointFilterInvocationContext which provides context for the execution,
    // and the EndpointFilterDelegate which represents the next filter or endpoint action to execute.
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        // Retrieve the user from the HttpContext associated with the current request.
        // HttpContext.User gives access to the security principal associated with this request.
        var user = context.HttpContext.User;

        // Check if the user's identity is authenticated.
        // The '?' checks if Identity is null before accessing IsAuthenticated to avoid a NullReferenceException.
        // '?? true' ensures that if Identity is null, the expression evaluates to true, treating it as not authenticated.
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            // If the user is not authenticated, return a 401 Unauthorized result immediately.
            // This stops further processing of the request pipeline and sends an unauthorized response to the client.
            return Results.Unauthorized();
        }

        // If the user is authenticated, continue executing the next filter or the endpoint action.
        // 'await next(context)' calls the next delegate in the pipeline, which could be another filter or the actual endpoint delegate.
        // This allows the request to proceed to the actual endpoint logic if all checks pass.
        return await next(context);
    }
}
