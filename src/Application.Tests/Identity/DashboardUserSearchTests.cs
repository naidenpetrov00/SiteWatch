using Application.Identity.Queries.DashboardUsers;
using Application.SeedWork.Interfaces;
using NSubstitute;

namespace Application.Tests.Identity;

public sealed class DashboardUserSearchTests
{
    [Fact]
    public async Task Validator_accepts_a_search_term_at_the_supported_length_limit()
    {
        var result = await new DashboardUserSearchQueryValidator().ValidateAsync(new DashboardUserSearchQuery
        {
            SearchTerm = new string('a', 200)
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validator_rejects_a_search_term_longer_than_the_supported_limit()
    {
        var result = await new DashboardUserSearchQueryValidator().ValidateAsync(new DashboardUserSearchQuery
        {
            SearchTerm = new string('a', 201)
        });

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(DashboardUserSearchQuery.SearchTerm));
    }

    [Fact]
    public async Task Handler_returns_the_identity_service_lookup_results_for_the_requested_search_term()
    {
        var identityService = Substitute.For<IIdentityService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var expected = new List<DashboardUserLookupDto>
        {
            new("user-1", "Ada Lovelace", "ada@example.com")
        };
        identityService.SearchUsersAsync("Ada", cancellationToken).Returns(Task.FromResult(expected));
        var handler = new DashboardUserSearchQueryHandler(identityService);

        var result = await handler.Handle(new DashboardUserSearchQuery { SearchTerm = "Ada" }, cancellationToken);

        Assert.Same(expected, result);
        await identityService.Received(1).SearchUsersAsync("Ada", cancellationToken);
    }

    [Fact]
    public async Task Handler_forwards_an_omitted_search_term_to_the_identity_service()
    {
        var identityService = Substitute.For<IIdentityService>();
        identityService.SearchUsersAsync(null, CancellationToken.None).Returns(Task.FromResult(new List<DashboardUserLookupDto>()));
        var handler = new DashboardUserSearchQueryHandler(identityService);

        await handler.Handle(new DashboardUserSearchQuery(), CancellationToken.None);

        await identityService.Received(1).SearchUsersAsync(null, CancellationToken.None);
    }
}
