using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Api.Services;
using Microsoft.AspNetCore.Http;

namespace Api.Tests.Services;

public sealed class CurrentUserTests
{
    [Fact]
    public void Reads_standard_name_identifier_and_email_claims()
    {
        var user = CreateUser(new Claim(ClaimTypes.NameIdentifier, "user-1"), new Claim(ClaimTypes.Email, "ada@example.test"));

        Assert.Equal("user-1", user.Id);
        Assert.Equal("ada@example.test", user.Email);
    }

    [Fact]
    public void Falls_back_to_JWT_subject_and_email_claims()
    {
        var user = CreateUser(new Claim(JwtRegisteredClaimNames.Sub, "user-2"), new Claim(JwtRegisteredClaimNames.Email, "bea@example.test"));

        Assert.Equal("user-2", user.Id);
        Assert.Equal("bea@example.test", user.Email);
    }

    [Fact]
    public void Returns_null_when_there_is_no_http_context()
    {
        var user = new CurrentUser(new HttpContextAccessor());

        Assert.Null(user.Id);
        Assert.Null(user.Email);
    }

    private static CurrentUser CreateUser(params Claim[] claims) => new(new HttpContextAccessor
    {
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
        }
    });
}
