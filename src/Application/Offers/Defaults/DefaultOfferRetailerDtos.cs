using Domain.Entities;

namespace Application.Offers.Defaults;

/// <summary>Represents one retailer in the ordered global defaults for new Offers.</summary>
public sealed record DefaultOfferRetailerDto(
    Guid RetailerId,
    string DisplayName,
    string BaseWebsiteUrl,
    string WebsiteHost)
{
    public static DefaultOfferRetailerDto From(Retailer retailer) =>
        new(
            retailer.Id,
            retailer.DisplayName,
            retailer.BaseWebsiteUrl,
            retailer.NormalizedWebsiteHost);
}
