using Application.Offers.Pricing;

namespace Application.SeedWork.Interfaces;

public interface IOfferPricingService
{
    Task<OfferPricingMatrixDto> GetMatrixAsync(
        Guid siteId,
        Guid offerId,
        CancellationToken cancellationToken);
    Task<OfferPricingMatrixDto> AddRetailerAsync(
        AddOfferRetailerCommand request,
        CancellationToken cancellationToken);
    Task RemoveRetailerAsync(
        Guid siteId,
        Guid offerId,
        Guid retailerId,
        CancellationToken cancellationToken);
    Task<OfferRetailerPriceCellDto> RecordManualPriceAsync(
        RecordManualRetailerPriceCommand request,
        CancellationToken cancellationToken);
    Task<OfferPricingProductRowDto> SelectPriceAsync(
        SelectOfferProductPriceCommand request,
        CancellationToken cancellationToken);
    Task ClearSelectionAsync(
        Guid siteId,
        Guid offerId,
        Guid offerProductLineId,
        CancellationToken cancellationToken);
    Task UpdateDiscountsAsync(
        UpdateOfferDiscountsCommand request,
        CancellationToken cancellationToken);
}
