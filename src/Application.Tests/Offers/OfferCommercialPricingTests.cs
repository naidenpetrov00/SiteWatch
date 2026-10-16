using Application.Offers.Commands;
using Application.Offers.Finalization;
using Application.Offers.Pricing;
using Domain.Entities;
using Domain.SeedWork.Enums;
using Domain.ValueObjects;

namespace Application.Tests.Offers;

public sealed class OfferCommercialPricingTests
{
    [Fact]
    public void Calculate_UsesFixedPerMeasurementAndFreePricingWithCategoryDiscounts()
    {
        var fixture = CreateOfferWithThreeSections();
        fixture.Offer.UpdateActivitySectionPricing(
            fixture.OfferActivity,
            new Dictionary<Guid, (ActivityPricingMode?, decimal?)>
            {
                [fixture.Sections[0].Id] = (ActivityPricingMode.Fixed, 100m),
                [fixture.Sections[1].Id] = (ActivityPricingMode.PerMeasurement, 5m),
                [fixture.Sections[2].Id] = (ActivityPricingMode.Free, null)
            });
        fixture.Offer.UpdateDiscounts(10m, 20m);
        AddSelectedProduct(fixture, requiredQuantity: 2m, optionalQuantity: 1m, price: 10m);

        var result = OfferCommercialTotals.Calculate(fixture.Offer);

        Assert.True(result.Totals.ActivityPricingComplete);
        Assert.True(result.Totals.ProductPricingComplete);
        Assert.Equal(150m, result.Totals.ActivitySubtotalBeforeDiscount);
        Assert.Equal(15m, result.Totals.ActivityDiscountAmount);
        Assert.Equal(135m, result.Totals.ActivityTotalAfterDiscount);
        Assert.Equal(30m, result.Totals.ProductSubtotalBeforeDiscount);
        Assert.Equal(6m, result.Totals.ProductDiscountAmount);
        Assert.Equal(24m, result.Totals.ProductTotalAfterDiscount);
        Assert.Equal(159m, result.Totals.CombinedOfferTotal);
    }

    [Fact]
    public void Readiness_DistinguishesMissingPricingFromExplicitlyFreePricing()
    {
        var fixture = CreateOfferWithThreeSections();

        var missing = OfferFinalizationReadiness.Evaluate(fixture.Offer, new HashSet<Guid>(), 0);

        Assert.False(missing.CanFinalize);
        Assert.Equal(3, missing.ActivitySectionsMissingPricing.Count);

        fixture.Offer.UpdateActivitySectionPricing(
            fixture.OfferActivity,
            fixture.Sections.ToDictionary(
                section => section.Id,
                _ => ((ActivityPricingMode?)ActivityPricingMode.Free, (decimal?)null)));

        var explicitlyFree = OfferFinalizationReadiness.Evaluate(
            fixture.Offer,
            new HashSet<Guid>(),
            0);

        Assert.True(explicitlyFree.CanFinalize);
        Assert.Empty(explicitlyFree.ActivitySectionsMissingPricing);
        Assert.Equal(3, explicitlyFree.ActivitySectionsWithPricing);
        Assert.Equal(0m, explicitlyFree.CommercialTotals.ActivityTotalAfterDiscount);
    }

    [Fact]
    public void PricingAndDiscounts_AreImmutableAfterFinalization()
    {
        var fixture = CreateOfferWithThreeSections();
        fixture.Offer.Finalize(DateTimeOffset.UtcNow, "administrator");

        Assert.Throws<InvalidOperationException>(() => fixture.Offer.UpdateActivitySectionPricing(
            fixture.OfferActivity,
            fixture.Sections.ToDictionary(
                section => section.Id,
                _ => ((ActivityPricingMode?)ActivityPricingMode.Free, (decimal?)null))));
        Assert.Throws<InvalidOperationException>(() => fixture.Offer.UpdateDiscounts(10m, 10m));
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    [InlineData(1.001)]
    public void DiscountValidator_RejectsOutOfRangeOrOverPrecisePercentages(double value)
    {
        var command = new UpdateOfferDiscountsCommand
        {
            SiteId = Guid.NewGuid(),
            OfferId = Guid.NewGuid(),
            ActivityDiscountPercentage = (decimal)value,
            ProductDiscountPercentage = 0m
        };

        var result = new UpdateOfferDiscountsValidator().Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ActivityPricingValidator_RequiresValidModeAmountPairs()
    {
        var validator = new OfferSectionPricingInputValidator();

        Assert.True(validator.Validate(new OfferSectionPricingInput(Guid.NewGuid(), null, null)).IsValid);
        Assert.True(validator.Validate(new OfferSectionPricingInput(Guid.NewGuid(), "free", null)).IsValid);
        Assert.True(validator.Validate(new OfferSectionPricingInput(Guid.NewGuid(), "fixed", 12.34m)).IsValid);
        Assert.True(validator.Validate(new OfferSectionPricingInput(Guid.NewGuid(), "per-measurement", 1m)).IsValid);
        Assert.False(validator.Validate(new OfferSectionPricingInput(Guid.NewGuid(), "free", 1m)).IsValid);
        Assert.False(validator.Validate(new OfferSectionPricingInput(Guid.NewGuid(), "fixed", null)).IsValid);
        Assert.False(validator.Validate(new OfferSectionPricingInput(Guid.NewGuid(), "future", 1m)).IsValid);
    }

    [Fact]
    public void ActivityPricing_RequiresExactlyOneEntryPerSnapshottedSection()
    {
        var fixture = CreateOfferWithThreeSections();

        Assert.Throws<InvalidOperationException>(() =>
            fixture.Offer.UpdateActivitySectionPricing(
                fixture.OfferActivity,
                new Dictionary<Guid, (ActivityPricingMode?, decimal?)>
                {
                    [fixture.Sections[0].Id] = (ActivityPricingMode.Fixed, 10m)
                }));

        var duplicateSectionId = fixture.Sections[0].Id;
        var command = new UpdateOfferActivitySectionPricingCommand
        {
            SiteId = Guid.NewGuid(),
            OfferId = Guid.NewGuid(),
            OfferActivityId = fixture.OfferActivity.Id,
            SectionPricing =
            [
                new(duplicateSectionId, "fixed", 10m),
                new(duplicateSectionId, "free", null)
            ]
        };

        Assert.False(new UpdateOfferActivitySectionPricingValidator().Validate(command).IsValid);
    }

    [Fact]
    public void Readiness_OptionalProductTotalFailureRemainsAWarning()
    {
        var fixture = CreateOfferWithThreeSections();
        fixture.Offer.UpdateActivitySectionPricing(
            fixture.OfferActivity,
            fixture.Sections.ToDictionary(
                section => section.Id,
                _ => ((ActivityPricingMode?)ActivityPricingMode.Free, (decimal?)null)));
        AddSelectedProduct(
            fixture,
            requiredQuantity: null,
            optionalQuantity: 2m,
            price: decimal.MaxValue);

        var readiness = OfferFinalizationReadiness.Evaluate(
            fixture.Offer,
            new HashSet<Guid>(),
            0);

        Assert.True(readiness.CanFinalize);
        Assert.Empty(readiness.BlockingReasons);
        Assert.Contains(readiness.Warnings, warning => warning.Contains("optional", StringComparison.OrdinalIgnoreCase));
        Assert.Null(readiness.CommercialTotals.CombinedOfferTotal);
    }

    private static OfferFixture CreateOfferWithThreeSections()
    {
        var site = new Site(
            (SiteName)"Commercial test site",
            (SiteAddress)"1 Test Street",
            "manager",
            new DateOnly(2026, 1, 1));
        var offer = Offer.Create(site);
        var activity = Activity.Create("Installation", null, null, 0);
        var sources = new[]
        {
            activity.AddRequirementSection("Fixed", 1m, ActivityMeasurementUnit.Piece, 0),
            activity.AddRequirementSection("Measured", 1m, ActivityMeasurementUnit.Meter, 1),
            activity.AddRequirementSection("Included", 1m, ActivityMeasurementUnit.Piece, 2)
        };
        var offerActivity = OfferActivity.Create(offer, activity, 0);
        var sections = new[]
        {
            offerActivity.AddSection(sources[0], 1m),
            offerActivity.AddSection(sources[1], 10m),
            offerActivity.AddSection(sources[2], 1m)
        };
        offer.AddActivity(offerActivity);
        return new OfferFixture(offer, offerActivity, sources, sections);
    }

    private static void AddSelectedProduct(
        OfferFixture fixture,
        decimal? requiredQuantity,
        decimal? optionalQuantity,
        decimal price)
    {
        var product = Product.Create(
            "Test product",
            null,
            null,
            null,
            null,
            null,
            ProductCategory.Other,
            ProductStatus.Active,
            ProductSearchConfiguration.Create(null));
        var line = OfferProductLine.Create(fixture.Offer, product);

        if (requiredQuantity.HasValue)
        {
            var requirement = fixture.SourceSections[0].AddProductRequirement(
                product,
                requiredQuantity.Value,
                true,
                ProductQuantityBehavior.Fixed,
                null,
                0);
            line.AddContribution(fixture.Sections[0], requirement);
        }

        if (optionalQuantity.HasValue)
        {
            var requirement = fixture.SourceSections[1].AddProductRequirement(
                product,
                optionalQuantity.Value,
                false,
                ProductQuantityBehavior.Fixed,
                null,
                0);
            line.AddContribution(fixture.Sections[1], requirement);
        }

        fixture.Offer.AddProductLine(line);
        var company = Person.CreateCompany("Retailer Ltd", null, "123456789", "BG123456789");
        var retailer = Retailer.Create("Retailer", company, "https://retailer.test", null);
        var comparison = fixture.Offer.AddRetailer(retailer);
        var listing = RetailerListing.Create(product, retailer, null, null);
        var now = DateTimeOffset.UtcNow;
        var observation = listing.RecordPrice(
            price,
            PriceBasis.Item,
            now,
            now,
            PriceObservationSource.Manual,
            null,
            "administrator");
        fixture.Offer.SelectProductPrice(
            line,
            comparison,
            listing,
            observation,
            now,
            "administrator");
    }

    private sealed record OfferFixture(
        Offer Offer,
        OfferActivity OfferActivity,
        ActivityRequirementSection[] SourceSections,
        OfferActivitySection[] Sections);
}
