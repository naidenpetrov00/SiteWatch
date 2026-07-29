using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Application.SeedWork.Security;
using Microsoft.IdentityModel.Tokens;

namespace Api.Tests;

public sealed class AuthorizationEndpointTests
{
    public static TheoryData<string?, HttpStatusCode> DashboardRoleCases => new()
    {
        { null, HttpStatusCode.Unauthorized },
        { UserRoles.Client, HttpStatusCode.Forbidden },
        { UserRoles.Worker, HttpStatusCode.Forbidden },
        { UserRoles.Administrator, HttpStatusCode.OK },
    };

    public static TheoryData<string?, HttpStatusCode> AssignmentRoleCases => new()
    {
        { null, HttpStatusCode.Unauthorized },
        { UserRoles.Client, HttpStatusCode.Forbidden },
        { UserRoles.Worker, HttpStatusCode.Forbidden },
        { UserRoles.Administrator, HttpStatusCode.NoContent },
    };

    [Theory]
    [MemberData(nameof(DashboardRoleCases))]
    public async Task DashboardUsers_enforces_administrator_role(
        string? role,
        HttpStatusCode expectedStatus
    )
    {
        using var factory = new SiteWatchApiFactory();
        using var client = CreateClient(factory, role);

        var response = await client.GetAsync("/dashboard/users?pageIndex=0&pageSize=10");

        Assert.Equal(expectedStatus, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AssignmentRoleCases))]
    public async Task SetUserRole_is_administrator_only(
        string? callerRole,
        HttpStatusCode expectedStatus
    )
    {
        using var factory = new SiteWatchApiFactory();
        using var client = CreateClient(factory, callerRole);

        var response = await client.PutAsJsonAsync(
            "/identity/users/target-user/role",
            new { role = UserRoles.Worker }
        );

        Assert.Equal(expectedStatus, response.StatusCode);
        if (expectedStatus == HttpStatusCode.NoContent)
        {
            Assert.Equal("target-user", factory.IdentityService.LastAssignedUserId);
            Assert.Equal(UserRoles.Worker, factory.IdentityService.LastAssignedRole);
        }
    }

    [Fact]
    public async Task SetUserRole_rejects_unknown_role()
    {
        using var factory = new SiteWatchApiFactory();
        using var client = CreateClient(factory, UserRoles.Administrator);

        var response = await client.PutAsJsonAsync(
            "/identity/users/target-user/role",
            new { role = "Unknown" }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(factory.IdentityService.LastAssignedRole);
    }

    [Theory]
    [InlineData("admin@example.test", HttpStatusCode.OK, UserRoles.Administrator)]
    [InlineData("client@example.test", HttpStatusCode.BadRequest, null)]
    [InlineData("worker@example.test", HttpStatusCode.BadRequest, null)]
    public async Task DashboardSignIn_accepts_only_administrators(
        string email,
        HttpStatusCode expectedStatus,
        string? expectedRole
    )
    {
        using var factory = new SiteWatchApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/dashboard/signIn",
            new { email, password = "Password1" }
        );

        Assert.Equal(expectedStatus, response.StatusCode);
        if (expectedRole is not null)
        {
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var roles = document.RootElement
                .GetProperty("user")
                .GetProperty("roles")
                .EnumerateArray()
                .Select(item => item.GetString())
                .ToList();
            Assert.Equal([expectedRole], roles);
        }
    }

    private static HttpClient CreateClient(SiteWatchApiFactory factory, string? role)
    {
        var client = factory.CreateClient();
        if (role is null)
            return client;

        var userId = role.ToLowerInvariant() + "-request-user";
        factory.IdentityService.SetRole(userId, role);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CreateToken(userId, role)
        );
        return client;
    }

    private static string CreateToken(string userId, string role)
    {
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SiteWatchApiFactory.SigningKey)),
            SecurityAlgorithms.HmacSha256
        );
        var token = new JwtSecurityToken(
            issuer: SiteWatchApiFactory.Issuer,
            audience: SiteWatchApiFactory.Audience,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.NameId, userId),
                new Claim(JwtRegisteredClaimNames.Email, userId + "@example.test"),
                new Claim(UserClaimTypes.UserType, role),
            ],
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
