using Application.Offers.Pricing;
using Application.RetailerPriceCollections;

namespace Application.SeedWork.Interfaces;

public interface IOfferOnlinePriceCollectionService
{
    Task<OfferOnlinePriceCollectionOptionsDto> GetOptionsAsync(
        Guid siteId,
        Guid offerId,
        CancellationToken cancellationToken);
    Task<OfferOnlinePriceCollectionStartResponseDto> StartAsync(
        StartOfferOnlinePriceCollectionCommand request,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<RetailerPriceCollectionRunSummaryDto>> GetRecentAsync(
        Guid siteId,
        Guid offerId,
        int limit,
        CancellationToken cancellationToken);
}
