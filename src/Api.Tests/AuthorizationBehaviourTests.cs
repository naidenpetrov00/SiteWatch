using Application.SeedWork.Behaviours;
using Application.SeedWork.Exceptions;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using MediatR;

namespace Api.Tests;

public sealed class AuthorizationBehaviourTests
{
    [Fact]
    public async Task Protected_request_rejects_anonymous_user()
    {
        var behaviour = new AuthorizationBehaviour<AdministratorRequest, string>(
            new TestUser(null),
            new TestIdentityService()
        );

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            behaviour.Handle(
                new AdministratorRequest(),
                _ => Task.FromResult("handled"),
                CancellationToken.None
            )
        );
    }

    [Theory]
    [InlineData(UserRoles.Administrator, true)]
    [InlineData(UserRoles.Client, false)]
    [InlineData(UserRoles.Worker, false)]
    public async Task Administrator_request_enforces_current_stored_role(
        string role,
        bool shouldSucceed
    )
    {
        const string userId = "request-user";
        var identity = new TestIdentityService();
        identity.SetRole(userId, role);
        var behaviour = new AuthorizationBehaviour<AdministratorRequest, string>(
            new TestUser(userId),
            identity
        );

        if (shouldSucceed)
        {
            var result = await behaviour.Handle(
                new AdministratorRequest(),
                _ => Task.FromResult("handled"),
                CancellationToken.None
            );
            Assert.Equal("handled", result);
        }
        else
        {
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                behaviour.Handle(
                    new AdministratorRequest(),
                    _ => Task.FromResult("handled"),
                    CancellationToken.None
                )
            );
        }
    }

    [Theory]
    [InlineData(UserRoles.Administrator, true)]
    [InlineData(UserRoles.Worker, true)]
    [InlineData(UserRoles.Client, false)]
    public async Task Combined_role_group_uses_or_semantics(string role, bool shouldSucceed)
    {
        const string userId = "combined-user";
        var identity = new TestIdentityService();
        identity.SetRole(userId, role);
        var behaviour = new AuthorizationBehaviour<AdministratorOrWorkerRequest, string>(
            new TestUser(userId),
            identity
        );

        var action = () => behaviour.Handle(
            new AdministratorOrWorkerRequest(),
            _ => Task.FromResult("handled"),
            CancellationToken.None
        );

        if (shouldSucceed)
            Assert.Equal("handled", await action());
        else
            await Assert.ThrowsAsync<ForbiddenAccessException>(action);
    }

    [Authorize(Roles = UserRoles.Administrator)]
    private sealed record AdministratorRequest : IRequest<string>;

    [Authorize(Roles = UserRoleGroups.AdministratorOrWorker)]
    private sealed record AdministratorOrWorkerRequest : IRequest<string>;

    private sealed record TestUser(string? Id) : IUser
    {
        public string? Email => Id is null ? null : Id + "@example.test";
    }
}
