using Application.RetailerListings;
using Application.SeedWork.Interfaces;
using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.SeedWork.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.RetailerListings.Services;

public sealed record RetailerListingPriceInput(decimal Amount, PriceBasis Basis);

public sealed class RetailerListingWriter(
    ApplicationDbContext dbContext,
    IUser user)
{
    public async Task<RetailerListing> CreateAsync(
        Guid productId,
        Guid retailerId,
        string? productUrl,
        string? retailerProductCode,
        RetailerListingPriceInput? price,
        CancellationToken cancellationToken)
    {
        var product = await dbContext.Products.SingleOrDefaultAsync(
            item => item.Id == productId,
            cancellationToken)
            ?? throw new NotFoundException(nameof(Product), productId.ToString());
        var retailer = await dbContext.Retailers.SingleOrDefaultAsync(
            item => item.Id == retailerId,
            cancellationToken)
            ?? throw new NotFoundException(nameof(Retailer), retailerId.ToString());

        EnsureCatalogEntriesActive(product, retailer);

        var duplicateExists = await dbContext.RetailerListings.AnyAsync(
            listing => listing.ProductId == productId
                && listing.RetailerId == retailerId,
            cancellationToken);
        if (duplicateExists)
        {
            throw new RetailerListingConflictException(
                "A listing already exists for this Product and Retailer.");
        }

        RetailerListing listing;
        try
        {
            listing = RetailerListing.Create(
                product,
                retailer,
                productUrl,
                retailerProductCode);
        }
        catch (ArgumentException exception)
        {
            throw new RetailerListingConflictException(exception.Message, exception);
        }

        SetCreatedAudit(listing);
        dbContext.RetailerListings.Add(listing);
        await RecordPriceIfChangedAsync(
            listing,
            product,
            retailer,
            price,
            cancellationToken);
        return listing;
    }

    public async Task<RetailerListing> UpsertOfferPriceAsync(
        Guid productId,
        Guid retailerId,
        string? productUrl,
        string? retailerProductCode,
        RetailerListingPriceInput price,
        CancellationToken cancellationToken)
    {
        var listing = await dbContext.RetailerListings
            .Include(item => item.Product)
            .Include(item => item.Retailer)
            .SingleOrDefaultAsync(
                item => item.ProductId == productId
                    && item.RetailerId == retailerId,
                cancellationToken);
        if (listing is null)
        {
            return await CreateAsync(
                productId,
                retailerId,
                productUrl,
                retailerProductCode,
                price,
                cancellationToken);
        }

        await UpdateAsync(
            listing,
            productUrl,
            retailerProductCode,
            price,
            cancellationToken);
        return listing;
    }

    public async Task UpdateAsync(
        RetailerListing listing,
        string? productUrl,
        string? retailerProductCode,
        RetailerListingPriceInput? price,
        CancellationToken cancellationToken)
    {
        try
        {
            listing.UpdateMetadata(productUrl, retailerProductCode);
        }
        catch (ArgumentException exception)
        {
            throw new RetailerListingConflictException(exception.Message, exception);
        }

        SetModifiedAudit(listing);
        await RecordPriceIfChangedAsync(
            listing,
            listing.Product,
            listing.Retailer,
            price,
            cancellationToken);
    }

    public void SetActive(RetailerListing listing, bool isActive)
    {
        if (isActive)
        {
            EnsureCatalogEntriesActive(listing.Product, listing.Retailer);
            listing.Activate();
        }
        else
        {
            listing.Deactivate();
        }

        SetModifiedAudit(listing);
    }

    private async Task RecordPriceIfChangedAsync(
        RetailerListing listing,
        Product product,
        Retailer retailer,
        RetailerListingPriceInput? price,
        CancellationToken cancellationToken)
    {
        if (price is null)
        {
            return;
        }

        EnsureCatalogEntriesActive(product, retailer);
        if (!listing.IsActive)
        {
            throw new RetailerListingConflictException(
                "Reactivate the retailer listing before recording a new price.");
        }

        EnsureSupportedBasis(product, price.Basis);
        var latest = await dbContext.RetailerPriceObservations
            .Where(observation => observation.RetailerListingId == listing.Id)
            .OrderByDescending(observation => observation.ObservedAt)
            .ThenByDescending(observation => observation.RecordedAt)
            .ThenByDescending(observation => observation.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (latest is not null
            && latest.Amount == price.Amount
            && latest.Basis == price.Basis)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        RetailerPriceObservation observation;
        try
        {
            observation = listing.RecordPrice(
                price.Amount,
                price.Basis,
                now,
                now,
                PriceObservationSource.Manual,
                null,
                GetUserId());
        }
        catch (InvalidOperationException exception)
        {
            throw new RetailerListingConflictException(exception.Message, exception);
        }

        dbContext.RetailerPriceObservations.Add(observation);
    }

    private static void EnsureCatalogEntriesActive(Product product, Retailer retailer)
    {
        if (product.Status != ProductStatus.Active)
        {
            throw new RetailerListingConflictException(
                "Only active Products can have an active retailer listing.");
        }

        if (!retailer.IsActive)
        {
            throw new RetailerListingConflictException(
                "Only active Retailers can have an active retailer listing.");
        }
    }

    private static void EnsureSupportedBasis(Product product, PriceBasis basis)
    {
        var supported = product.PackageQuantity.HasValue
            ? product.PackageUnit == ProductPackageUnit.Piece
                || basis == PriceBasis.Package
            : basis == PriceBasis.Item;
        if (!supported)
        {
            throw new RetailerListingConflictException(
                product.PackageQuantity.HasValue
                    ? "Item pricing is supported only for packages measured in pieces."
                    : "Package pricing requires Product package metadata.");
        }
    }

    private void SetCreatedAudit(RetailerListing listing)
    {
        var now = DateTimeOffset.UtcNow;
        var userId = GetUserId();
        listing.Created = now;
        listing.CreatedBy = userId;
        listing.LastModified = now;
        listing.LastModifiedBy = userId;
    }

    private void SetModifiedAudit(RetailerListing listing)
    {
        listing.LastModified = DateTimeOffset.UtcNow;
        listing.LastModifiedBy = GetUserId();
    }

    private string GetUserId() => user.Id ?? throw new UnauthorizedAccessException();
}
