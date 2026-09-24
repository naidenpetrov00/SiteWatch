using System.Data;
using Application.Offers;
using Application.Offers.Pricing;
using Application.SeedWork.Interfaces;
using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.SeedWork.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Offers.Services;

public sealed class OfferPricingService(ApplicationDbContext dbContext, IUser user)
    : IOfferPricingService
{
    public async Task<OfferPricingMatrixDto> GetMatrixAsync(
        Guid siteId,
        Guid offerId,
        CancellationToken cancellationToken)
    {
        var offer = await LoadMatrixOfferAsync(siteId, offerId, cancellationToken);
        var productIds = offer.ProductLines.Select(line => line.ProductId).ToList();
        var retailerIds = offer.RetailerComparisons.Select(item => item.RetailerId).ToList();

        List<ListingLatest> listings = productIds.Count == 0 || retailerIds.Count == 0
            ? []
            : await dbContext.RetailerListings
                .AsNoTracking()
                .Where(listing => productIds.Contains(listing.ProductId)
                    && retailerIds.Contains(listing.RetailerId))
                .Select(listing => new ListingLatest(
                    listing.Id,
                    listing.ProductId,
                    listing.RetailerId,
                    listing.ProductUrl,
                    listing.RetailerProductCode,
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
                        .FirstOrDefault()))
                .ToListAsync(cancellationToken);

        return BuildMatrix(offer, listings);
    }

    public async Task<OfferPricingMatrixDto> AddRetailerAsync(
        AddOfferRetailerCommand request,
        CancellationToken cancellationToken)
    {
        await ExecuteMutationAsync(
            async () =>
            {
                var offer = await LoadTrackedOfferAsync(
                    request.SiteId,
                    request.OfferId,
                    cancellationToken);
                EnsureDraft(offer);
                var retailer = await dbContext.Retailers.SingleOrDefaultAsync(
                    item => item.Id == request.RetailerId,
                    cancellationToken);
                if (retailer is null)
                {
                    throw new NotFoundException(
                        nameof(Retailer),
                        request.RetailerId.ToString());
                }

                if (!retailer.IsActive)
                {
                    throw new OfferConflictException(
                        "Only active retailers can be added to an Offer.");
                }

                try
                {
                    var comparison = offer.AddRetailer(retailer);
                    dbContext.OfferRetailerComparisons.Add(comparison);
                }
                catch (InvalidOperationException exception)
                {
                    throw new OfferConflictException(exception.Message, exception);
                }

                SetOfferAudit(offer);
            },
            "The retailer could not be added because the Offer changed.",
            cancellationToken);

        return await GetMatrixAsync(request.SiteId, request.OfferId, cancellationToken);
    }

    public Task RemoveRetailerAsync(
        Guid siteId,
        Guid offerId,
        Guid retailerId,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var offer = await LoadTrackedOfferAsync(siteId, offerId, cancellationToken);
                EnsureDraft(offer);
                var comparison = offer.RetailerComparisons.SingleOrDefault(
                    item => item.RetailerId == retailerId);
                if (comparison is null)
                {
                    throw new NotFoundException(
                        nameof(OfferRetailerComparison),
                        retailerId.ToString());
                }

                try
                {
                    offer.RemoveRetailer(comparison);
                    dbContext.OfferRetailerComparisons.Remove(comparison);
                }
                catch (InvalidOperationException exception)
                {
                    throw new OfferConflictException(exception.Message, exception);
                }

                SetOfferAudit(offer);
            },
            "The retailer could not be removed because the Offer changed.",
            cancellationToken);

    public async Task<OfferRetailerPriceCellDto> RecordManualPriceAsync(
        RecordManualRetailerPriceCommand request,
        CancellationToken cancellationToken)
    {
        await ExecuteMutationAsync(
            async () =>
            {
                var offer = await LoadTrackedOfferAsync(
                    request.SiteId,
                    request.OfferId,
                    cancellationToken);
                EnsureDraft(offer);
                var line = GetLine(offer, request.OfferProductLineId);
                var comparison = GetComparison(offer, request.RetailerId);
                if (!PriceBasisCodes.TryParse(request.Basis, out var basis))
                {
                    throw new ArgumentException("Unsupported price basis.", nameof(request));
                }

                try
                {
                    _ = line.CalculatePriceTotal(1m, request.Amount, basis);
                }
                catch (InvalidOperationException exception)
                {
                    throw new OfferConflictException(exception.Message, exception);
                }

                var listing = await dbContext.RetailerListings
                    .SingleOrDefaultAsync(
                        item => item.ProductId == line.ProductId
                            && item.RetailerId == request.RetailerId,
                        cancellationToken);
                var now = DateTimeOffset.UtcNow;
                var userId = GetUserId();
                if (listing is null)
                {
                    var product = await dbContext.Products.SingleOrDefaultAsync(
                        item => item.Id == line.ProductId,
                        cancellationToken)
                        ?? throw new NotFoundException(
                            nameof(Product),
                            line.ProductId.ToString());
                    try
                    {
                        listing = RetailerListing.Create(
                            product,
                            comparison.Retailer,
                            request.ProductUrl,
                            request.RetailerProductCode);
                    }
                    catch (ArgumentException exception)
                    {
                        throw new OfferConflictException(exception.Message, exception);
                    }
                    listing.Created = now;
                    listing.CreatedBy = userId;
                    listing.LastModified = now;
                    listing.LastModifiedBy = userId;
                    dbContext.RetailerListings.Add(listing);
                }
                else
                {
                    try
                    {
                        listing.UpdateMetadata(
                            request.ProductUrl,
                            request.RetailerProductCode);
                    }
                    catch (ArgumentException exception)
                    {
                        throw new OfferConflictException(exception.Message, exception);
                    }

                    listing.LastModified = now;
                    listing.LastModifiedBy = userId;
                }

                var latest = await dbContext.RetailerPriceObservations
                    .Where(observation => observation.RetailerListingId == listing.Id)
                    .OrderByDescending(observation => observation.ObservedAt)
                    .ThenByDescending(observation => observation.RecordedAt)
                    .ThenByDescending(observation => observation.Id)
                    .FirstOrDefaultAsync(cancellationToken);
                if (latest is null
                    || latest.Amount != request.Amount
                    || latest.Basis != basis)
                {
                    var observation = listing.RecordPrice(
                        request.Amount,
                        basis,
                        now,
                        now,
                        PriceObservationSource.Manual,
                        null,
                        userId);
                    dbContext.RetailerPriceObservations.Add(observation);
                }
            },
            "The price could not be saved because its listing changed.",
            cancellationToken);

        var matrix = await GetMatrixAsync(request.SiteId, request.OfferId, cancellationToken);
        return matrix.Products
            .Single(row => row.OfferProductLineId == request.OfferProductLineId)
            .RetailerPrices
            .Single(cell => cell.RetailerId == request.RetailerId);
    }

    public async Task<OfferPricingProductRowDto> SelectPriceAsync(
        SelectOfferProductPriceCommand request,
        CancellationToken cancellationToken)
    {
        await ExecuteMutationAsync(
            async () =>
            {
                var offer = await LoadTrackedOfferAsync(
                    request.SiteId,
                    request.OfferId,
                    cancellationToken);
                EnsureDraft(offer);
                var line = GetLine(offer, request.OfferProductLineId);
                var comparison = GetComparison(offer, request.RetailerId);
                var listing = await dbContext.RetailerListings
                    .SingleOrDefaultAsync(
                        item => item.ProductId == line.ProductId
                            && item.RetailerId == request.RetailerId,
                        cancellationToken)
                    ?? throw new NotFoundException(
                        nameof(RetailerListing),
                        request.RetailerId.ToString());
                var latest = await dbContext.RetailerPriceObservations
                    .Where(observation => observation.RetailerListingId == listing.Id)
                    .OrderByDescending(observation => observation.ObservedAt)
                    .ThenByDescending(observation => observation.RecordedAt)
                    .ThenByDescending(observation => observation.Id)
                    .FirstOrDefaultAsync(cancellationToken)
                    ?? throw new NotFoundException(
                        nameof(RetailerPriceObservation),
                        request.ObservationId.ToString());
                if (latest.Id != request.ObservationId)
                {
                    throw new OfferConflictException(
                        "A newer price is available. Refresh and select the latest observation.");
                }

                var previousSelection = line.PriceSelection;
                try
                {
                    offer.SelectProductPrice(
                        line,
                        comparison,
                        listing,
                        latest,
                        DateTimeOffset.UtcNow,
                        GetUserId());
                }
                catch (InvalidOperationException exception)
                {
                    throw new OfferConflictException(exception.Message, exception);
                }

                if (previousSelection is not null)
                {
                    dbContext.OfferProductPriceSelections.Remove(previousSelection);
                }

                dbContext.OfferProductPriceSelections.Add(line.PriceSelection!);
                SetOfferAudit(offer);
            },
            "The price could not be selected because the Offer or listing changed.",
            cancellationToken);

        var matrix = await GetMatrixAsync(request.SiteId, request.OfferId, cancellationToken);
        return matrix.Products.Single(row =>
            row.OfferProductLineId == request.OfferProductLineId);
    }

    public Task ClearSelectionAsync(
        Guid siteId,
        Guid offerId,
        Guid offerProductLineId,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var offer = await LoadTrackedOfferAsync(siteId, offerId, cancellationToken);
                EnsureDraft(offer);
                var line = GetLine(offer, offerProductLineId);
                var selection = line.PriceSelection;
                if (selection is null)
                {
                    return;
                }

                offer.ClearProductPrice(line);
                dbContext.OfferProductPriceSelections.Remove(selection);
                SetOfferAudit(offer);
            },
            "The selected price could not be cleared because the Offer changed.",
            cancellationToken);

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
        var observations = observationEntities
            .Select(observation => new OfferPriceObservationDto(
                observation.Id,
                observation.Amount,
                observation.CurrencyCode,
                observation.Basis.ToCode(),
                observation.ObservedAt,
                observation.RecordedAt,
                observation.Source.ToCode(),
                observation.SourceReference,
                observation.RecordedBy))
            .ToList();

        return new RetailerPriceHistoryDto(
            productId,
            retailerId,
            listing.Id,
            listing.ProductUrl,
            listing.RetailerProductCode,
            observations,
            pageIndex,
            pageSize,
            totalCount);
    }

    private async Task<Offer> LoadMatrixOfferAsync(
        Guid siteId,
        Guid offerId,
        CancellationToken cancellationToken)
    {
        var offer = await dbContext.Offers
            .AsNoTrackingWithIdentityResolution()
            .Include(item => item.RetailerComparisons)
            .ThenInclude(comparison => comparison.Retailer)
            .Include(item => item.ProductLines)
            .ThenInclude(line => line.PriceSelection)
            .Include(item => item.ProductLines)
            .ThenInclude(line => line.Contributions)
            .ThenInclude(contribution => contribution.OfferActivitySection)
            .ThenInclude(section => section.OfferActivity)
            .AsSplitQuery()
            .SingleOrDefaultAsync(
                item => item.Id == offerId && item.SiteId == siteId,
                cancellationToken);
        return offer ?? throw new NotFoundException(nameof(Offer), offerId.ToString());
    }

    private async Task<Offer> LoadTrackedOfferAsync(
        Guid siteId,
        Guid offerId,
        CancellationToken cancellationToken)
    {
        var offer = await dbContext.Offers
            .Include(item => item.RetailerComparisons)
            .ThenInclude(comparison => comparison.Retailer)
            .Include(item => item.ProductLines)
            .ThenInclude(line => line.PriceSelection)
            .AsSplitQuery()
            .SingleOrDefaultAsync(
                item => item.Id == offerId && item.SiteId == siteId,
                cancellationToken);
        return offer ?? throw new NotFoundException(nameof(Offer), offerId.ToString());
    }

    private static OfferPricingMatrixDto BuildMatrix(
        Offer offer,
        IReadOnlyCollection<ListingLatest> listings)
    {
        var listingsByPair = listings.ToDictionary(
            item => (item.ProductId, item.RetailerId));
        var retailers = offer.RetailerComparisons
            .OrderBy(item => item.RetailerDisplayName)
            .ThenBy(item => item.RetailerId)
            .Select(item => new OfferPricingRetailerDto(
                item.RetailerId,
                item.RetailerDisplayName,
                item.RetailerBaseWebsiteUrl,
                item.Retailer.IsActive))
            .ToList();
        var rows = offer.ProductLines
            .OrderBy(line => line.ProductNumberId)
            .ThenBy(line => line.Id)
            .Select(line => BuildRow(line, retailers, listingsByPair))
            .ToList();

        var requiredComplete = rows.All(row =>
            row.RequiredQuantity <= 0m || row.SelectedRequiredTotal.HasValue);
        var optionalComplete = rows.All(row =>
            row.OptionalQuantity <= 0m || row.SelectedOptionalTotal.HasValue);
        return new OfferPricingMatrixDto(
            offer.Id,
            offer.Status.ToString(),
            RetailerPriceObservation.EuroCurrencyCode,
            requiredComplete,
            optionalComplete,
            requiredComplete ? rows.Sum(row => row.SelectedRequiredTotal ?? 0m) : null,
            optionalComplete ? rows.Sum(row => row.SelectedOptionalTotal ?? 0m) : null,
            retailers,
            rows);
    }

    private static OfferPricingProductRowDto BuildRow(
        OfferProductLine line,
        IReadOnlyList<OfferPricingRetailerDto> retailers,
        IReadOnlyDictionary<(Guid ProductId, Guid RetailerId), ListingLatest> listings)
    {
        var cells = retailers.Select(retailer =>
        {
            listings.TryGetValue((line.ProductId, retailer.RetailerId), out var listing);
            return BuildCell(line, retailer.RetailerId, listing);
        }).ToList();
        var comparisonQuantity = line.RequiredQuantity > 0m
            ? line.RequiredQuantity
            : line.OptionalQuantity;
        var comparableTotals = cells
            .Select(cell => line.RequiredQuantity > 0m
                ? cell.ComparableRequiredTotal
                : cell.ComparableOptionalTotal)
            .Where(total => total.HasValue)
            .Select(total => total!.Value)
            .ToList();
        if (comparisonQuantity > 0m && comparableTotals.Count > 0)
        {
            var cheapest = comparableTotals.Min();
            cells = cells.Select(cell => cell with
            {
                IsCheapest = (line.RequiredQuantity > 0m
                        ? cell.ComparableRequiredTotal
                        : cell.ComparableOptionalTotal) == cheapest
            }).ToList();
        }

        var requirements = line.Contributions
            .OrderBy(contribution => contribution.OfferActivitySection.OfferActivity.SortOrder)
            .ThenBy(contribution => contribution.OfferActivitySection.SortOrder)
            .ThenBy(contribution => contribution.SortOrder)
            .ThenBy(contribution => contribution.Id)
            .Select(contribution => new OfferPricingRequirementDto(
                contribution.Id,
                contribution.OfferActivitySection.OfferActivityId,
                contribution.OfferActivitySection.OfferActivity.ActivityNumberId,
                contribution.OfferActivitySection.OfferActivity.Name,
                contribution.OfferActivitySection.OfferActivity.SortOrder,
                contribution.OfferActivitySectionId,
                contribution.OfferActivitySection.Name,
                contribution.OfferActivitySection.SortOrder,
                contribution.OfferActivitySection.BasisQuantity,
                contribution.OfferActivitySection.MeasurementUnit.ToCode(),
                contribution.OfferActivitySection.RequestedMeasurement,
                contribution.ConfiguredQuantity,
                contribution.IsRequired,
                contribution.QuantityBehavior.ToCode(),
                contribution.Notes,
                contribution.SortOrder,
                contribution.CalculatedQuantity))
            .ToList();

        var selection = line.PriceSelection;
        decimal? selectedRequiredTotal = null;
        decimal? selectedOptionalTotal = null;
        OfferSelectedPriceDto? selectedPrice = null;
        if (selection is not null)
        {
            selectedRequiredTotal = TryCalculate(
                line,
                line.RequiredQuantity,
                selection.Amount,
                selection.Basis);
            selectedOptionalTotal = TryCalculate(
                line,
                line.OptionalQuantity,
                selection.Amount,
                selection.Basis);
            var newestForRetailer = cells.SingleOrDefault(
                cell => cell.RetailerId == selection.RetailerId)?.LatestObservation;
            selectedPrice = new OfferSelectedPriceDto(
                selection.RetailerId,
                selection.RetailerListingId,
                selection.RetailerPriceObservationId,
                selection.RetailerDisplayName,
                selection.ProductUrl,
                selection.RetailerProductCode,
                selection.Amount,
                selection.CurrencyCode,
                selection.Basis.ToCode(),
                selection.ObservedAt,
                selection.RecordedAt,
                selection.Source.ToCode(),
                selection.SourceReference,
                selection.RecordedBy,
                selection.SelectedAt,
                selection.SelectedBy,
                newestForRetailer is not null
                    && newestForRetailer.Id != selection.RetailerPriceObservationId);
        }

        return new OfferPricingProductRowDto(
            line.Id,
            line.ProductId,
            line.ProductNumberId,
            line.Title,
            line.Category.ToCode(),
            line.Brand,
            line.Model,
            line.PackageQuantity,
            line.PackageUnit?.ToCode(),
            line.RequiredQuantity,
            line.OptionalQuantity,
            requirements,
            selectedRequiredTotal,
            selectedOptionalTotal,
            selectedPrice,
            cells);
    }

    private static OfferRetailerPriceCellDto BuildCell(
        OfferProductLine line,
        Guid retailerId,
        ListingLatest? listing)
    {
        var latest = listing?.Latest;
        return new OfferRetailerPriceCellDto(
            line.Id,
            line.ProductId,
            retailerId,
            listing?.Id,
            listing?.ProductUrl,
            listing?.RetailerProductCode,
            latest is null ? null : ToDto(latest),
            latest is null
                ? null
                : TryCalculate(line, line.RequiredQuantity, latest.Amount, latest.Basis),
            latest is null
                ? null
                : TryCalculate(line, line.OptionalQuantity, latest.Amount, latest.Basis),
            false);
    }

    private static OfferPriceObservationDto ToDto(ObservationProjection observation) =>
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

    private static decimal? TryCalculate(
        OfferProductLine line,
        decimal quantity,
        decimal amount,
        PriceBasis basis)
    {
        try
        {
            return line.CalculatePriceTotal(quantity, amount, basis);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static OfferProductLine GetLine(Offer offer, Guid lineId) =>
        offer.ProductLines.SingleOrDefault(line => line.Id == lineId)
        ?? throw new NotFoundException(nameof(OfferProductLine), lineId.ToString());

    private static OfferRetailerComparison GetComparison(Offer offer, Guid retailerId) =>
        offer.RetailerComparisons.SingleOrDefault(item => item.RetailerId == retailerId)
        ?? throw new OfferConflictException(
            "Add the retailer to this Offer before recording or selecting its price.");

    private static void EnsureDraft(Offer offer)
    {
        if (offer.Status != OfferStatus.Draft)
        {
            throw new OfferConflictException("Only draft offers can be edited.");
        }
    }

    private string GetUserId() => user.Id ?? throw new UnauthorizedAccessException();

    private void SetOfferAudit(Offer offer)
    {
        offer.LastModified = DateTimeOffset.UtcNow;
        offer.LastModifiedBy = GetUserId();
    }

    private async Task ExecuteMutationAsync(
        Func<Task> mutation,
        string persistenceConflictMessage,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        try
        {
            await mutation();
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw new OfferConflictException(persistenceConflictMessage, exception);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private sealed record ListingLatest(
        Guid Id,
        Guid ProductId,
        Guid RetailerId,
        string? ProductUrl,
        string? RetailerProductCode,
        ObservationProjection? Latest);

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
