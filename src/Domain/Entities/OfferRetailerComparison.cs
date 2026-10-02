using Domain.SeedWork;

namespace Domain.Entities;

public sealed class OfferRetailerComparison : BaseEntity
{
    private OfferRetailerComparison()
    {
    }

    public Guid OfferId { get; private set; }
    public Offer Offer { get; private set; } = null!;
    public Guid RetailerId { get; private set; }
    public Retailer Retailer { get; private set; } = null!;
    public string RetailerDisplayName { get; private set; } = string.Empty;
    public string RetailerBaseWebsiteUrl { get; private set; } = string.Empty;

    public static OfferRetailerComparison Create(Offer offer, Retailer retailer)
    {
        ArgumentNullException.ThrowIfNull(offer);
        ArgumentNullException.ThrowIfNull(retailer);

        return new OfferRetailerComparison
        {
            Id = Guid.NewGuid(),
            Offer = offer,
            OfferId = offer.Id,
            Retailer = retailer,
            RetailerId = retailer.Id,
            RetailerDisplayName = retailer.DisplayName,
            RetailerBaseWebsiteUrl = retailer.BaseWebsiteUrl
        };
    }
}

