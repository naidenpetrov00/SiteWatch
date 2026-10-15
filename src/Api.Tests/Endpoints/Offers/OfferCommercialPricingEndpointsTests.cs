using System.Net;
using System.Net.Http.Json;
using Api.Tests.Infrastructure;
using Application.Offers.Commands;
using Application.Offers.Pricing;
using NSubstitute;

namespace Api.Tests.Endpoints.Offers;

public sealed class OfferCommercialPricingEndpointsTests
{
    [Fact]
    public async Task Commercial_pricing_mutations_use_route_identifiers_and_bind_complete_requests()
    {
        await using var factory = new SiteWatchApiFactory();
        UpdateOfferActivitySectionPricingCommand? pricing = null;
        UpdateOfferDiscountsCommand? discounts = null;
        factory.Mediator.Send(
                Arg.Do<UpdateOfferActivitySectionPricingCommand>(command => pricing = command),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        factory.Mediator.Send(
                Arg.Do<UpdateOfferDiscountsCommand>(command => discounts = command),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        using var client = factory.CreateHttpsClient();
        var siteId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var offerId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var offerActivityId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var firstSectionId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var secondSectionId = Guid.Parse("55555555-5555-5555-5555-555555555555");

        var pricingResponse = await client.PutAsJsonAsync(
            $"/sites/{siteId}/offers/{offerId}/activities/{offerActivityId}/section-pricing",
            new
            {
                siteId = Guid.NewGuid(),
                offerId = Guid.NewGuid(),
                offerActivityId = Guid.NewGuid(),
                sectionPricing = new object[]
                {
                    new { sectionId = firstSectionId, pricingMode = "fixed", priceAmount = (decimal?)125.50m },
                    new { sectionId = secondSectionId, pricingMode = "free", priceAmount = (decimal?)null }
                }
            },
            TestContext.Current.CancellationToken);
        var discountsResponse = await client.PutAsJsonAsync(
            $"/sites/{siteId}/offers/{offerId}/discounts",
            new
            {
                siteId = Guid.NewGuid(),
                offerId = Guid.NewGuid(),
                activityDiscountPercentage = 10m,
                productDiscountPercentage = 15.5m
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, pricingResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, discountsResponse.StatusCode);
        Assert.Equal(siteId, pricing?.SiteId);
        Assert.Equal(offerId, pricing?.OfferId);
        Assert.Equal(offerActivityId, pricing?.OfferActivityId);
        Assert.Collection(
            pricing!.SectionPricing,
            item =>
            {
                Assert.Equal(firstSectionId, item.SectionId);
                Assert.Equal("fixed", item.PricingMode);
                Assert.Equal(125.50m, item.PriceAmount);
            },
            item =>
            {
                Assert.Equal(secondSectionId, item.SectionId);
                Assert.Equal("free", item.PricingMode);
                Assert.Null(item.PriceAmount);
            });
        Assert.Equal(siteId, discounts?.SiteId);
        Assert.Equal(offerId, discounts?.OfferId);
        Assert.Equal(10m, discounts?.ActivityDiscountPercentage);
        Assert.Equal(15.5m, discounts?.ProductDiscountPercentage);
    }
}
