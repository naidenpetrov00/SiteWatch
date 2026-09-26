using Application.RetailerListings;
using Application.SeedWork.Models;

namespace Application.SeedWork.Interfaces;

public interface IRetailerListingService
{
    Task<PagedResult<RetailerListingDto>> GetForProductAsync(
        ProductRetailerListingsQuery request,
        CancellationToken cancellationToken);
    Task<PagedResult<RetailerListingDto>> GetForRetailerAsync(
        RetailerProductListingsQuery request,
        CancellationToken cancellationToken);
    Task<RetailerListingDto> GetByIdAsync(
        Guid listingId,
        CancellationToken cancellationToken);
    Task<RetailerListingDto> CreateAsync(
        CreateRetailerListingCommand request,
        CancellationToken cancellationToken);
    Task<RetailerListingDto> UpdateAsync(
        UpdateRetailerListingCommand request,
        CancellationToken cancellationToken);
    Task<RetailerListingDto> SetActiveAsync(
        Guid listingId,
        bool isActive,
        CancellationToken cancellationToken);
    Task<RetailerPriceHistoryDto> GetHistoryAsync(
        Guid productId,
        Guid retailerId,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken);
}
