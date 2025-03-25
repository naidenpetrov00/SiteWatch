using Api.SeedWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public class User : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this.GetType().Name);

        group.MapGet("/test", Test).RequireAuthorization();
    }

    private static Ok Test(HttpContext context)
    {
        return TypedResults.Ok();
    }
}
