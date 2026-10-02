using System.Data;
using Application.RetailerListings;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.SeedWork.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.RetailerListings.Services;

public sealed class RetailerListingService(
    ApplicationDbContext dbContext,
    RetailerListingWriter writer) : IRetailerListingService
{
    public async Task<PagedResult<RetailerListingDto>> GetForProductAsync(
        ProductRetailerListingsQuery request,
        CancellationToken cancellationToken)
    {
        var productExists = await dbContext.Products.AsNoTracking().AnyAsync(
            product => product.Id == request.ProductId,
            cancellationToken);
        if (!productExists)
        {
            throw new NotFoundException(nameof(Product), request.ProductId.ToString());
        }

        var baseQuery = dbContext.RetailerListings
            .AsNoTracking()
            .Where(listing => listing.ProductId == request.ProductId);
        return await GetPageAsync(
            baseQuery,
            request.SearchTerm,
            request.IncludeInactive,
            request.IsActive,
            request.PageIndex,
            request.PageSize,
            request.SortActive,
            request.SortDirection,
            productPerspective: true,
            cancellationToken);
    }

    public async Task<PagedResult<RetailerListingDto>> GetForRetailerAsync(
        RetailerProductListingsQuery request,
        CancellationToken cancellationToken)
    {
        var retailerExists = await dbContext.Retailers.AsNoTracking().AnyAsync(
            retailer => retailer.Id == request.RetailerId,
            cancellationToken);
        if (!retailerExists)
        {
            throw new NotFoundException(nameof(Retailer), request.RetailerId.ToString());
        }

        var baseQuery = dbContext.RetailerListings
            .AsNoTracking()
            .Where(listing => listing.RetailerId == request.RetailerId);
        return await GetPageAsync(
            baseQuery,
            request.SearchTerm,
            request.IncludeInactive,
            request.IsActive,
            request.PageIndex,
            request.PageSize,
            request.SortActive,
            request.SortDirection,
            productPerspective: false,
            cancellationToken);
    }

    public async Task<PagedResult<RetailerListingDto>> GetForCompanyAsync(
        CompanyRetailerListingsQuery request,
        CancellationToken cancellationToken)
    {
        var eligibleCompanyExists = await dbContext.Persons.AsNoTracking().AnyAsync(
            person => person.Id == request.CompanyPersonId
                && person.Type == PersonType.Company
                && person.Retailers.Any(),
            cancellationToken);
        if (!eligibleCompanyExists)
        {
            throw new NotFoundException(nameof(Person), request.CompanyPersonId.ToString());
        }

        var baseQuery = dbContext.RetailerListings
            .AsNoTracking()
            .Where(listing => listing.Retailer.CompanyPersonId == request.CompanyPersonId);
        return await GetPageAsync(
            baseQuery,
            request.SearchTerm,
            request.IncludeInactive,
            request.IsActive,
            request.PageIndex,
            request.PageSize,
            request.SortActive,
            request.SortDirection,
            productPerspective: false,
            cancellationToken);
    }

    public async Task<RetailerListingDto> GetByIdAsync(
        Guid listingId,
        CancellationToken cancellationToken)
    {
        var projection = await Project(dbContext.RetailerListings
                .AsNoTracking()
                .Where(listing => listing.Id == listingId))
            .SingleOrDefaultAsync(cancellationToken);
        return projection is null
            ? throw new NotFoundException(nameof(RetailerListing), listingId.ToString())
            : ToDto(projection);
    }

    public async Task<RetailerListingDto> CreateAsync(
        CreateRetailerListingCommand request,
        CancellationToken cancellationToken)
    {
        var price = ParsePrice(request.Amount, request.Basis);
        var listingId = await ExecuteMutationAsync(
            async () =>
            {
                var listing = await writer.CreateAsync(
                    request.ProductId,
                    request.RetailerId,
                    request.ProductUrl,
                    request.RetailerProductCode,
                    price,
                    cancellationToken);
                return listing.Id;
            },
            "The retailer listing could not be created because it changed.",
            cancellationToken);
        return await GetByIdAsync(listingId, cancellationToken);
    }

    public async Task<RetailerListingDto> UpdateAsync(
        UpdateRetailerListingCommand request,
        CancellationToken cancellationToken)
    {
        var price = ParsePrice(request.Amount, request.Basis);
        await ExecuteMutationAsync(
            async () =>
            {
                var listing = await LoadTrackedAsync(
                    request.ListingId,
                    cancellationToken);
                await writer.UpdateAsync(
                    listing,
                    request.ProductUrl,
                    request.RetailerProductCode,
                    price,
                    cancellationToken);
                return true;
            },
            "The retailer listing could not be updated because it changed.",
            cancellationToken);
        return await GetByIdAsync(request.ListingId, cancellationToken);
    }

    public async Task<RetailerListingDto> SetActiveAsync(
        Guid listingId,
        bool isActive,
        CancellationToken cancellationToken)
    {
        await ExecuteMutationAsync(
            async () =>
            {
                var listing = await LoadTrackedAsync(listingId, cancellationToken);
                writer.SetActive(listing, isActive);
                return true;
            },
            "The retailer listing status could not be changed.",
            cancellationToken);
        return await GetByIdAsync(listingId, cancellationToken);
    }

    public async Task<RetailerPriceHistoryDto> GetHistoryAsync(
        Guid productId,
        Guid retailerId,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var productExists = await dbContext.Products.AsNoTracking().AnyAsync(
            product => product.Id == productId,
            cancellationToken);
        if (!productExists)
        {
            throw new NotFoundException(nameof(Product), productId.ToString());
        }

        var retailerExists = await dbContext.Retailers.AsNoTracking().AnyAsync(
            retailer => retailer.Id == retailerId,
            cancellationToken);
        if (!retailerExists)
        {
            throw new NotFoundException(nameof(Retailer), retailerId.ToString());
        }

        var listing = await dbContext.RetailerListings
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.ProductId == productId && item.RetailerId == retailerId,
                cancellationToken);
        if (listing is null)
        {
            return new RetailerPriceHistoryDto(
                productId,
                retailerId,
                null,
                null,
                null,
                null,
                [],
                pageIndex,
                pageSize,
                0);
        }

        var query = dbContext.RetailerPriceObservations
            .AsNoTracking()
            .Where(observation => observation.RetailerListingId == listing.Id);
        var totalCount = await query.CountAsync(cancellationToken);
        var observationEntities = await query
            .OrderByDescending(observation => observation.ObservedAt)
            .ThenByDescending(observation => observation.RecordedAt)
            .ThenByDescending(observation => observation.Id)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new RetailerPriceHistoryDto(
            productId,
            retailerId,
            listing.Id,
            listing.IsActive,
            listing.ProductUrl,
            listing.RetailerProductCode,
            observationEntities.Select(ToDto).ToList(),
            pageIndex,
            pageSize,
            totalCount);
    }

    private async Task<PagedResult<RetailerListingDto>> GetPageAsync(
        IQueryable<RetailerListing> baseQuery,
        string? searchTerm,
        bool includeInactive,
        bool? isActive,
        int pageIndex,
        int pageSize,
        string? sortActive,
        string? sortDirection,
        bool productPerspective,
        CancellationToken cancellationToken)
    {
        var totalCount = await baseQuery.CountAsync(cancellationToken);
        var filtered = includeInactive
            ? baseQuery
            : baseQuery.Where(listing => listing.IsActive);
        if (isActive.HasValue)
        {
            filtered = filtered.Where(listing => listing.IsActive == isActive.Value);
        }
        var search = searchTerm?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var numberSearch = int.TryParse(search, out var numberId);
            filtered = productPerspective
                ? filtered.Where(listing =>
                    listing.Retailer.DisplayName.Contains(search)
                    || listing.Retailer.NormalizedWebsiteHost.Contains(search)
                    || (listing.RetailerProductCode != null
                        && listing.RetailerProductCode.Contains(search)))
                : filtered.Where(listing =>
                    listing.Product.Title.Contains(search)
                    || (listing.Product.Brand != null
                        && listing.Product.Brand.Contains(search))
                    || (listing.Product.Model != null
                        && listing.Product.Model.Contains(search))
                    || (listing.RetailerProductCode != null
                        && listing.RetailerProductCode.Contains(search))
                    || numberSearch && listing.Product.NumberId == numberId);
        }

        var filteredCount = await filtered.CountAsync(cancellationToken);
        var sorted = ApplySort(
            filtered,
            sortActive,
            sortDirection,
            productPerspective);
        var projections = await Project(sorted)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return new PagedResult<RetailerListingDto>(
            projections.Select(ToDto).ToList(),
            filteredCount,
            totalCount);
    }

    private static IQueryable<RetailerListing> ApplySort(
        IQueryable<RetailerListing> query,
        string? sortActive,
        string? sortDirection,
        bool productPerspective)
    {
        var key = sortActive?.Trim().ToLowerInvariant();
        var descending = sortDirection?.Equals(
            "desc",
            StringComparison.OrdinalIgnoreCase) == true;

        return (key, descending) switch
        {
            ("retailer", false) => query
                .OrderBy(listing => listing.Retailer.DisplayName)
                .ThenBy(listing => listing.Id),
            ("retailer", true) => query
                .OrderByDescending(listing => listing.Retailer.DisplayName)
                .ThenBy(listing => listing.Id),
            ("productnumberid", false) => query
                .OrderBy(listing => listing.Product.NumberId)
                .ThenBy(listing => listing.Id),
            ("productnumberid", true) => query
                .OrderByDescending(listing => listing.Product.NumberId)
                .ThenBy(listing => listing.Id),
            ("product", false) => query
                .OrderBy(listing => listing.Product.Title)
                .ThenBy(listing => listing.Id),
            ("product", true) => query
                .OrderByDescending(listing => listing.Product.Title)
                .ThenBy(listing => listing.Id),
            ("latestprice", false) => query
                .OrderBy(listing => listing.PriceObservations
                    .OrderByDescending(observation => observation.ObservedAt)
                    .ThenByDescending(observation => observation.RecordedAt)
                    .ThenByDescending(observation => observation.Id)
                    .Select(observation => (decimal?)observation.Amount)
                    .FirstOrDefault())
                .ThenBy(listing => listing.Id),
            ("latestprice", true) => query
                .OrderByDescending(listing => listing.PriceObservations
                    .OrderByDescending(observation => observation.ObservedAt)
                    .ThenByDescending(observation => observation.RecordedAt)
                    .ThenByDescending(observation => observation.Id)
                    .Select(observation => (decimal?)observation.Amount)
                    .FirstOrDefault())
                .ThenBy(listing => listing.Id),
            ("observedat", false) => query
                .OrderBy(listing => listing.PriceObservations
                    .OrderByDescending(observation => observation.ObservedAt)
                    .ThenByDescending(observation => observation.RecordedAt)
                    .ThenByDescending(observation => observation.Id)
                    .Select(observation => (DateTimeOffset?)observation.ObservedAt)
                    .FirstOrDefault())
                .ThenBy(listing => listing.Id),
            ("observedat", true) => query
                .OrderByDescending(listing => listing.PriceObservations
                    .OrderByDescending(observation => observation.ObservedAt)
                    .ThenByDescending(observation => observation.RecordedAt)
                    .ThenByDescending(observation => observation.Id)
                    .Select(observation => (DateTimeOffset?)observation.ObservedAt)
                    .FirstOrDefault())
                .ThenBy(listing => listing.Id),
            ("retailerproductcode", false) => query
                .OrderBy(listing => listing.RetailerProductCode ?? string.Empty)
                .ThenBy(listing => listing.Id),
            ("retailerproductcode", true) => query
                .OrderByDescending(listing => listing.RetailerProductCode ?? string.Empty)
                .ThenBy(listing => listing.Id),
            ("isactive", false) => query
                .OrderBy(listing => listing.IsActive)
                .ThenBy(listing => listing.Id),
            ("isactive", true) => query
                .OrderByDescending(listing => listing.IsActive)
                .ThenBy(listing => listing.Id),
            _ when productPerspective => query
                .OrderBy(listing => listing.Retailer.DisplayName)
                .ThenBy(listing => listing.Id),
            _ => query
                .OrderBy(listing => listing.Product.Title)
                .ThenBy(listing => listing.Id)
        };
    }

    private async Task<RetailerListing> LoadTrackedAsync(
        Guid listingId,
        CancellationToken cancellationToken) =>
        await dbContext.RetailerListings
            .Include(listing => listing.Product)
            .Include(listing => listing.Retailer)
            .SingleOrDefaultAsync(
                listing => listing.Id == listingId,
                cancellationToken)
        ?? throw new NotFoundException(nameof(RetailerListing), listingId.ToString());

    private static RetailerListingPriceInput? ParsePrice(
        decimal? amount,
        string? basis)
    {
        if (!amount.HasValue)
        {
            return null;
        }

        if (!PriceBasisCodes.TryParse(basis, out var parsedBasis))
        {
            throw new RetailerListingConflictException(
                "Basis must be item or package when an amount is supplied.");
        }

        return new RetailerListingPriceInput(amount.Value, parsedBasis);
    }

    private async Task<TResult> ExecuteMutationAsync<TResult>(
        Func<Task<TResult>> mutation,
        string persistenceConflictMessage,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        try
        {
            var result = await mutation();
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch (DbUpdateException exception)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw new RetailerListingConflictException(
                persistenceConflictMessage,
                exception);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private static IQueryable<ListingProjection> Project(
        IQueryable<RetailerListing> query) =>
        query.Select(listing => new ListingProjection(
            listing.Id,
            listing.ProductId,
            listing.Product.NumberId,
            listing.Product.Title,
            listing.Product.Status,
            listing.Product.Brand,
            listing.Product.Model,
            listing.Product.PackageQuantity,
            listing.Product.PackageUnit,
            listing.RetailerId,
            listing.Retailer.DisplayName,
            listing.Retailer.BaseWebsiteUrl,
            listing.Retailer.IsActive,
            listing.IsActive,
            listing.ProductUrl,
            listing.RetailerProductCode,
            listing.Created,
            listing.LastModified,
            listing.PriceObservations
                .OrderByDescending(observation => observation.ObservedAt)
                .ThenByDescending(observation => observation.RecordedAt)
                .ThenByDescending(observation => observation.Id)
                .Select(observation => new ObservationProjection(
                    observation.Id,
                    observation.Amount,
                    observation.CurrencyCode,
                    observation.Basis,
                    observation.ObservedAt,
                    observation.RecordedAt,
                    observation.Source,
                    observation.SourceReference,
                    observation.RecordedBy))
                .FirstOrDefault()));

    private static RetailerListingDto ToDto(ListingProjection listing) =>
        new(
            listing.Id,
            listing.ProductId,
            listing.ProductNumberId,
            listing.ProductTitle,
            listing.ProductStatus.ToString(),
            listing.ProductBrand,
            listing.ProductModel,
            listing.ProductPackageQuantity,
            listing.ProductPackageUnit?.ToCode(),
            listing.RetailerId,
            listing.RetailerDisplayName,
            listing.RetailerBaseWebsiteUrl,
            listing.RetailerIsActive,
            listing.IsActive,
            listing.ProductUrl,
            listing.RetailerProductCode,
            listing.Created,
            listing.LastModified,
            listing.LatestObservation is null
                ? null
                : ToDto(listing.LatestObservation));

    private static RetailerPriceObservationDto ToDto(
        RetailerPriceObservation observation) =>
        new(
            observation.Id,
            observation.Amount,
            observation.CurrencyCode,
            observation.Basis.ToCode(),
            observation.ObservedAt,
            observation.RecordedAt,
            observation.Source.ToCode(),
            observation.SourceReference,
            observation.RecordedBy);

    private static RetailerPriceObservationDto ToDto(
        ObservationProjection observation) =>
        new(
            observation.Id,
            observation.Amount,
            observation.CurrencyCode,
            observation.Basis.ToCode(),
            observation.ObservedAt,
            observation.RecordedAt,
            observation.Source.ToCode(),
            observation.SourceReference,
            observation.RecordedBy);

    private sealed record ListingProjection(
        Guid Id,
        Guid ProductId,
        int ProductNumberId,
        string ProductTitle,
        ProductStatus ProductStatus,
        string? ProductBrand,
        string? ProductModel,
        decimal? ProductPackageQuantity,
        ProductPackageUnit? ProductPackageUnit,
        Guid RetailerId,
        string RetailerDisplayName,
        string RetailerBaseWebsiteUrl,
        bool RetailerIsActive,
        bool IsActive,
        string? ProductUrl,
        string? RetailerProductCode,
        DateTimeOffset Created,
        DateTimeOffset LastModified,
        ObservationProjection? LatestObservation);

    private sealed record ObservationProjection(
        Guid Id,
        decimal Amount,
        string CurrencyCode,
        PriceBasis Basis,
        DateTimeOffset ObservedAt,
        DateTimeOffset RecordedAt,
        PriceObservationSource Source,
        string? SourceReference,
        string RecordedBy);
}
