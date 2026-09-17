using Application.Retailers.Commands;
using Application.Retailers.Queries;
using Application.Persons.Queries;
using Application.SeedWork.Interfaces;
using NSubstitute;

namespace Application.Tests.Retailers;

public sealed class RetailerApplicationTests
{
    [Fact]
    public async Task Command_handlers_forward_the_request_and_cancellation_to_the_retailer_service()
    {
        var retailers = Substitute.For<IRetailerService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var create = new CreateRetailerCommand { DisplayName = "Example", CompanyPersonId = Guid.NewGuid(), BaseWebsiteUrl = "https://example.com" };
        var update = new UpdateRetailerCommand { Id = Guid.NewGuid(), DisplayName = "Updated", CompanyPersonId = Guid.NewGuid(), BaseWebsiteUrl = "https://updated.example" };
        retailers.CreateAsync(create, cancellationToken).Returns(Guid.Parse("11111111-1111-1111-1111-111111111111"));

        var id = await new CreateRetailerHandler(retailers).Handle(create, cancellationToken);
        await new UpdateRetailerHandler(retailers).Handle(update, cancellationToken);
        await new SetRetailerActiveHandler(retailers).Handle(new SetRetailerActiveCommand(update.Id, false), cancellationToken);

        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), id);
        await retailers.Received(1).CreateAsync(create, cancellationToken);
        await retailers.Received(1).UpdateAsync(update, cancellationToken);
        await retailers.Received(1).SetActiveAsync(update.Id, false, cancellationToken);
    }

    [Fact]
    public async Task Search_validators_enforce_the_public_term_and_identifier_boundaries()
    {
        var retailerSearch = await new RetailerSearchQueryValidator().ValidateAsync(new RetailerSearchQuery { SearchTerm = new string('x', 201) });
        var companySearch = await new CompanyPersonSearchQueryValidator().ValidateAsync(new CompanyPersonSearchQuery { SearchTerm = new string('x', 201) });
        var status = await new SetRetailerActiveValidator().ValidateAsync(new SetRetailerActiveCommand(Guid.Empty, false));

        Assert.Contains(retailerSearch.Errors, error => error.PropertyName == nameof(RetailerSearchQuery.SearchTerm));
        Assert.Contains(companySearch.Errors, error => error.PropertyName == nameof(CompanyPersonSearchQuery.SearchTerm));
        Assert.Contains(status.Errors, error => error.PropertyName == nameof(SetRetailerActiveCommand.Id));
    }
}
