using Application.RetailerPriceCollections;

namespace Application.SeedWork.Interfaces;

public interface IRetailerPriceCollectionService
{
    Task<RetailerPriceCollectionRunSummaryDto> StartAsync(
        Guid companyPersonId,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<RetailerPriceCollectionRunSummaryDto>> GetRecentAsync(
        Guid companyPersonId,
        int limit,
        CancellationToken cancellationToken);
    Task<RetailerPriceCollectionRunDetailsDto> GetByIdAsync(
        Guid companyPersonId,
        Guid runId,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken);
}

public interface IRetailerPriceCollectionProcessor
{
    Task<bool> TryAcquireLeadershipAsync(
        Guid ownerId,
        DateTimeOffset leaseExpiresAt,
        CancellationToken cancellationToken);
    Task<bool> RenewLeadershipAsync(
        Guid ownerId,
        DateTimeOffset leaseExpiresAt,
        CancellationToken cancellationToken);
    Task ReleaseLeadershipAsync(Guid ownerId, CancellationToken cancellationToken);
    Task<bool> TryProcessNextAsync(Guid leaderId, CancellationToken cancellationToken);
}
