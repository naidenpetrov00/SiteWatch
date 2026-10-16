using Domain.Entities;
using Domain.SeedWork.Enums;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Infrastructure.Tests.Offers;

public sealed class OfferPricingPersistenceMetadataTests
{
    [Fact]
    public void Offer_pricing_mapping_preserves_discount_and_activity_price_contracts()
    {
        using var dbContext = CreateDbContext();
        var offer = dbContext.Model.FindEntityType(typeof(Offer))!;
        var section = dbContext.Model.FindEntityType(typeof(OfferActivitySection))!;

        var activityDiscount = offer.FindProperty(nameof(Offer.ActivityDiscountPercentage))!;
        var productDiscount = offer.FindProperty(nameof(Offer.ProductDiscountPercentage))!;
        var pricingMode = section.FindProperty(nameof(OfferActivitySection.PricingMode))!;
        var priceAmount = section.FindProperty(nameof(OfferActivitySection.PriceAmount))!;

        Assert.Equal(5, activityDiscount.GetPrecision());
        Assert.Equal(2, activityDiscount.GetScale());
        Assert.Equal(0m, activityDiscount.GetDefaultValue());
        Assert.Equal(5, productDiscount.GetPrecision());
        Assert.Equal(2, productDiscount.GetScale());
        Assert.Equal(0m, productDiscount.GetDefaultValue());
        Assert.True(pricingMode.IsNullable);
        Assert.Equal(
            "per-measurement",
            pricingMode.GetValueConverter()!.ConvertToProvider(ActivityPricingMode.PerMeasurement));
        Assert.Equal(18, priceAmount.GetPrecision());
        Assert.Equal(2, priceAmount.GetScale());
        Assert.True(priceAmount.IsNullable);
        Assert.Contains(offer.GetCheckConstraints(), constraint =>
            constraint.Name == "CK_Offers_ActivityDiscountPercentage");
        Assert.Contains(offer.GetCheckConstraints(), constraint =>
            constraint.Name == "CK_Offers_ProductDiscountPercentage");
        Assert.Contains(section.GetCheckConstraints(), constraint =>
            constraint.Name == "CK_OfferActivitySections_Pricing");
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=SiteWatchOfferModelMetadata;Trusted_Connection=True;")
            .Options;

        return new ApplicationDbContext(options, Substitute.For<IMediator>());
    }
}
