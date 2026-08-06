using Application.Identity.Commands.SetUserRole;
using Application.Identity.Commands;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using NSubstitute;

namespace Application.Tests.Identity;

public sealed class SetUserRoleCommandTests
{
    [Theory]
    [InlineData(UserRoles.Administrator)]
    [InlineData(UserRoles.Client)]
    [InlineData(UserRoles.Worker)]
    public async Task Validator_accepts_supported_roles(string role)
    {
        var result = await new SetUserRoleCommandValidator().ValidateAsync(new SetUserRoleCommand
        {
            UserId = "user-1", Role = role
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validator_rejects_unknown_roles()
    {
        var result = await new SetUserRoleCommandValidator().ValidateAsync(new SetUserRoleCommand
        {
            UserId = "user-1", Role = "Supervisor"
        });

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(SetUserRoleCommand.Role));
    }

    [Fact]
    public async Task Handler_assigns_the_requested_supported_role()
    {
        var identityService = Substitute.For<IIdentityService>();
        identityService.AssignRoleAsync("user-1", UserRoles.Worker)
            .Returns(Task.FromResult<IdentityResultModel>(null!));
        var handler = new SetUserRoleHandler(identityService);
        var command = new SetUserRoleCommand { UserId = "user-1", Role = UserRoles.Worker };

        await handler.Handle(command, CancellationToken.None);

        await identityService.Received(1).AssignRoleAsync("user-1", UserRoles.Worker);
    }

    [Fact]
    public void Command_is_restricted_to_administrators()
    {
        var authorization = Assert.Single(typeof(SetUserRoleCommand)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>());

        Assert.Equal(UserRoles.Administrator, authorization.Roles);
    }
}
