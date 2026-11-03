using Application.Offers.Defaults;

namespace Application.SeedWork.Interfaces;

public interface IDefaultOfferRetailerService
{
    Task<IReadOnlyList<DefaultOfferRetailerDto>> GetAsync(
        CancellationToken cancellationToken);

    Task<IReadOnlyList<DefaultOfferRetailerDto>> ReplaceAsync(
        IReadOnlyList<Guid> retailerIds,
        CancellationToken cancellationToken);
}
