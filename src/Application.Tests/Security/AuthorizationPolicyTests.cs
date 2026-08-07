using Application.Invoices.Commands;
using Application.Invoices.Queries;
using Application.Persons.Commands;
using Application.SeedWork.Security;
using Application.Sites.Commands;

namespace Application.Tests.Security;

public sealed class AuthorizationPolicyTests
{
    [Theory]
    [InlineData(typeof(CreateInvoiceCommand), UserRoles.Administrator)]
    [InlineData(typeof(CreatePersonCommand), UserRoles.Administrator)]
    [InlineData(typeof(CreateSiteCommand), UserRoles.Administrator)]
    [InlineData(typeof(CreateInvoiceFromFileCommand), UserRoleGroups.AdministratorOrWorker)]
    [InlineData(typeof(SiteInvoicesQuery), UserRoleGroups.AdministratorOrWorker)]
    public void Changed_use_cases_declare_the_expected_role_contract(Type useCaseType, string expectedRoles)
    {
        var authorization = Assert.Single(useCaseType
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>());

        Assert.Equal(expectedRoles, authorization.Roles);
    }

    [Fact]
    public void Supported_roles_are_exactly_the_dashboard_and_client_role_set()
    {
        Assert.Equal(
            [UserRoles.Administrator, UserRoles.Client, UserRoles.Worker],
            UserRoles.All.OrderBy(role => role));
        Assert.False(UserRoles.IsSupported("Supervisor"));
    }
}
