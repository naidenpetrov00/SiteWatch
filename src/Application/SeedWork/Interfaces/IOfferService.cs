using Application.Offers.Commands;
using Application.Offers.Queries;
using Application.SeedWork.Models;

namespace Application.SeedWork.Interfaces;

public interface IOfferService
{
    Task<Guid> CreateAsync(Guid siteId, CancellationToken cancellationToken);
    Task UpdateMetadataAsync(
        UpdateOfferMetadataCommand request,
        CancellationToken cancellationToken);
    Task ArchiveAsync(
        Guid siteId,
        Guid offerId,
        CancellationToken cancellationToken);
    Task<OfferDetailsDto> GetByIdAsync(
        Guid siteId,
        Guid offerId,
        CancellationToken cancellationToken);
    Task<PagedResult<OfferSummaryDto>> GetBySiteAsync(
        SiteOffersQuery request,
        CancellationToken cancellationToken);
}
