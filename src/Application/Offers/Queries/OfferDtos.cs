using Domain.Entities;

namespace Application.Offers.Queries;

/// <summary>Represents an offer in a site-scoped dashboard list.</summary>
public sealed record OfferSummaryDto(
    Guid Id,
    int NumberId,
    Guid SiteId,
    string? Title,
    string Status,
    DateTimeOffset Created,
    DateTimeOffset LastModified)
{
    public static OfferSummaryDto From(Offer offer) => new(
        offer.Id,
        offer.NumberId,
        offer.SiteId,
        offer.Title,
        offer.Status.ToString(),
        offer.Created,
        offer.LastModified);
}

/// <summary>Represents an offer and its associated site in the offer workspace.</summary>
public sealed record OfferDetailsDto(
    Guid Id,
    int NumberId,
    Guid SiteId,
    int SiteNumberId,
    string SiteName,
    string SiteAddress,
    string? Title,
    string? Notes,
    string Status,
    DateTimeOffset Created,
    string? CreatedBy,
    DateTimeOffset LastModified,
    string? LastModifiedBy)
{
    public static OfferDetailsDto From(Offer offer) => new(
        offer.Id,
        offer.NumberId,
        offer.SiteId,
        offer.Site.NumberId,
        offer.Site.Name.Value,
        offer.Site.Address.Value,
        offer.Title,
        offer.Notes,
        offer.Status.ToString(),
        offer.Created,
        offer.CreatedBy,
        offer.LastModified,
        offer.LastModifiedBy);
}
