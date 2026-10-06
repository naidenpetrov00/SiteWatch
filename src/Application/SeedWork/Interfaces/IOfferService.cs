using Application.Offers.Commands;
using Application.Offers.Finalization;
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
    Task FinalizeAsync(
        Guid siteId,
        Guid offerId,
        CancellationToken cancellationToken);
    Task<OfferFinalizationReadinessDto> GetReadinessAsync(
        Guid siteId,
        Guid offerId,
        CancellationToken cancellationToken);
    Task<Guid> AddActivityAsync(
        AddOfferActivityCommand request,
        CancellationToken cancellationToken);
    Task RemoveActivityAsync(
        Guid siteId,
        Guid offerId,
        Guid offerActivityId,
        CancellationToken cancellationToken);
    Task UpdateActivityMeasurementsAsync(
        UpdateOfferActivityMeasurementsCommand request,
        CancellationToken cancellationToken);
    Task UpdateActivitySectionPricingAsync(
        UpdateOfferActivitySectionPricingCommand request,
        CancellationToken cancellationToken);
    Task<OfferDetailsDto> GetByIdAsync(
        Guid siteId,
        Guid offerId,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<OfferActivityCatalogNodeDto>> GetActivityCatalogAsync(
        OfferActivityCatalogQuery request,
        CancellationToken cancellationToken);
    Task<OfferActivityCandidateDto> GetActivityCandidateAsync(
        Guid siteId,
        Guid offerId,
        Guid activityId,
        CancellationToken cancellationToken);
    Task<PagedResult<OfferSummaryDto>> GetBySiteAsync(
        SiteOffersQuery request,
        CancellationToken cancellationToken);
}
