using Application.SeedWork.Queries;
using Domain.Entities;
using Domain.SeedWork.Enums;

namespace Application.Offers.Queries;

public sealed partial class SiteOffersQuery
{
    public static readonly TableQueryDefinition<Offer, SiteOffersQuery> Table =
        new(
            Filters:
            [
                TableFilterDescriptorExtensions.IntEquals<Offer, SiteOffersQuery>(
                    "numberId",
                    query => query.NumberId,
                    offer => offer.NumberId),
                TableFilterDescriptor<Offer, SiteOffersQuery>.TextContains(
                    "title",
                    query => query.Title,
                    offer => offer.Title ?? string.Empty),
                new TableFilterDescriptor<Offer, SiteOffersQuery>(
                    "status",
                    query => Enum.TryParse<OfferStatus>(query.Status, true, out var status)
                        && Enum.IsDefined(status)
                        ? offer => offer.Status == status
                        : null)
            ],
            Sorts: new Dictionary<string, TableSortDescriptor<Offer, SiteOffersQuery>>(
                StringComparer.OrdinalIgnoreCase)
            {
                ["numberId"] = TableSortDescriptor<Offer, SiteOffersQuery>.Create(
                    "numberId", offer => offer.NumberId, offer => offer.Id),
                ["title"] = TableSortDescriptor<Offer, SiteOffersQuery>.Create(
                    "title", offer => offer.Title ?? string.Empty, offer => offer.Id),
                ["status"] = TableSortDescriptor<Offer, SiteOffersQuery>.Create(
                    "status", offer => offer.Status, offer => offer.Id),
                ["created"] = TableSortDescriptor<Offer, SiteOffersQuery>.Create(
                    "created", offer => offer.Created, offer => offer.Id),
                ["lastModified"] = TableSortDescriptor<Offer, SiteOffersQuery>.Create(
                    "lastModified", offer => offer.LastModified, offer => offer.Id)
            },
            DefaultSort: query => query
                .OrderByDescending(offer => offer.NumberId)
                .ThenBy(offer => offer.Id));
}
