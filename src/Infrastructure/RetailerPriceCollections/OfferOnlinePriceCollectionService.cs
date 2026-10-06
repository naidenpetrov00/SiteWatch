using System.Data;
using Application.Offers;
using Application.Offers.Pricing;
using Application.RetailerPriceCollections;
using Application.SeedWork.Interfaces;
using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.SeedWork.Enums;
using FluentValidation;
using FluentValidation.Results;
using Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.RetailerPriceCollections;

public sealed class OfferOnlinePriceCollectionService(
    ApplicationDbContext dbContext,
    IUser user) : IOfferOnlinePriceCollectionService
{
    public async Task<OfferOnlinePriceCollectionOptionsDto> GetOptionsAsync(
        Guid siteId,
        Guid offerId,
        CancellationToken cancellationToken)
    {
        var offer = await LoadOfferAsync(siteId, offerId, tracked: false, cancellationToken);
        EnsureDraft(offer);
        var companyIds = offer.RetailerComparisons
            .Select(comparison => comparison.Retailer.CompanyPersonId)
            .Distinct()
            .ToList();
        var companies = new List<OfferOnlinePriceCollectionCompanyOptionDto>();
        foreach (var companyId in companyIds)
        {
            companies.Add(await BuildCompanyOptionAsync(offer, companyId, cancellationToken));
        }

        return new OfferOnlinePriceCollectionOptionsDto(
            offer.Id,
            companies
                .OrderBy(company => company.CompanyDisplayName)
                .ThenBy(company => company.CompanyPersonId)
                .ToList());
    }

    public async Task<OfferOnlinePriceCollectionStartResponseDto> StartAsync(
        StartOfferOnlinePriceCollectionCommand request,
        CancellationToken cancellationToken)
    {
        var offer = await LoadOfferAsync(
            request.SiteId,
            request.OfferId,
            tracked: false,
            cancellationToken);
        EnsureDraft(offer);
        var representedCompanyIds = offer.RetailerComparisons
            .Select(comparison => comparison.Retailer.CompanyPersonId)
            .ToHashSet();
        var invalidCompanyIds = request.CompanyPersonIds
            .Where(companyId => !representedCompanyIds.Contains(companyId))
            .ToList();
        if (invalidCompanyIds.Count > 0)
        {
            throw new ValidationException([
                new ValidationFailure(
                    nameof(request.CompanyPersonIds),
                    "Every selected company must own a current retailer comparison column for the offer.")
            ]);
        }

        var outcomes = new List<OfferOnlinePriceCollectionStartOutcomeDto>();
        foreach (var companyId in request.CompanyPersonIds)
        {
            outcomes.Add(await StartCompanyAsync(
                request.SiteId,
                request.OfferId,
                companyId,
                cancellationToken));
        }

        return new OfferOnlinePriceCollectionStartResponseDto(request.OfferId, outcomes);
    }

    public async Task<IReadOnlyList<RetailerPriceCollectionRunSummaryDto>> GetRecentAsync(
        Guid siteId,
        Guid offerId,
        int limit,
        CancellationToken cancellationToken)
    {
        _ = await LoadOfferAsync(siteId, offerId, tracked: false, cancellationToken);
        var runs = await dbContext.RetailerPriceCollectionRuns
            .AsNoTracking()
            .Include(run => run.CompanyPerson)
            .Include(run => run.ExtractionProfile)
            .Where(run => run.OfferId == offerId)
            .OrderByDescending(run => run.RequestedAt)
            .ThenByDescending(run => run.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);
        return runs.Select(run => RetailerPriceCollectionService.ToSummary(
            run,
            run.CompanyPerson.DisplayName,
            run.ExtractionProfile.Version)).ToList();
    }

    private async Task<OfferOnlinePriceCollectionStartOutcomeDto> StartCompanyAsync(
        Guid siteId,
        Guid offerId,
        Guid companyPersonId,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        try
        {
            var offer = await LoadOfferAsync(siteId, offerId, tracked: true, cancellationToken);
            if (offer.Status != OfferStatus.Draft)
            {
                return await RollbackStateChangedAsync(
                    transaction,
                    companyPersonId,
                    "Offer is no longer a draft.",
                    [],
                    cancellationToken);
            }

            var option = await BuildCompanyOptionAsync(offer, companyPersonId, cancellationToken);
            if (!option.CanStart)
            {
                return await RollbackStateChangedAsync(
                    transaction,
                    option.CompanyPersonId,
                    BuildUnavailableMessage(option.BlockingCodes),
                    option.Exclusions,
                    cancellationToken,
                    option.CompanyDisplayName);
            }

            var company = await dbContext.Persons.SingleAsync(
                person => person.Id == companyPersonId && person.Type == PersonType.Company,
                cancellationToken);
            var profile = await dbContext.RetailerExtractionProfiles.SingleAsync(
                profile => profile.CompanyPersonId == companyPersonId
                    && profile.Status == RetailerExtractionProfileStatus.Published
                    && profile.IsActive,
                cancellationToken);
            var eligibleListingIds = await GetEligibleListingIdsAsync(
                offer,
                companyPersonId,
                cancellationToken);
            var listings = await dbContext.RetailerListings
                .Include(listing => listing.Retailer)
                .Where(listing => eligibleListingIds.Contains(listing.Id))
                .OrderBy(listing => listing.Retailer.DisplayName)
                .ThenBy(listing => listing.ProductId)
                .ThenBy(listing => listing.Id)
                .ToListAsync(cancellationToken);

            var now = DateTimeOffset.UtcNow;
            var userId = user.Id ?? throw new UnauthorizedAccessException();
            var run = RetailerPriceCollectionRun.Create(
                company,
                profile,
                listings,
                now,
                userId,
                offer);
            run.Created = now;
            run.CreatedBy = userId;
            run.LastModified = now;
            run.LastModifiedBy = userId;
            offer.LastModified = now;
            offer.LastModifiedBy = userId;
            dbContext.RetailerPriceCollectionRuns.Add(run);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
            return new OfferOnlinePriceCollectionStartOutcomeDto(
                company.Id,
                company.DisplayName,
                "accepted",
                "accepted",
                "Price collection was queued.",
                RetailerPriceCollectionService.ToSummary(run, company.DisplayName, profile.Version),
                option.Exclusions);
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            dbContext.ChangeTracker.Clear();
            return await StateChangedAsync(companyPersonId, cancellationToken);
        }
        catch (SqlException exception) when (exception.Number is 1205 or 1222)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            dbContext.ChangeTracker.Clear();
            return await StateChangedAsync(companyPersonId, cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            dbContext.ChangeTracker.Clear();
            throw;
        }
    }

    private async Task<OfferOnlinePriceCollectionStartOutcomeDto> StateChangedAsync(
        Guid companyPersonId,
        CancellationToken cancellationToken)
    {
        var companyName = await dbContext.Persons
            .AsNoTracking()
            .Where(person => person.Id == companyPersonId)
            .Select(person => person.DisplayName)
            .SingleOrDefaultAsync(cancellationToken) ?? "Retailer company";
        return new OfferOnlinePriceCollectionStartOutcomeDto(
            companyPersonId,
            companyName,
            "unavailable",
            "stateChanged",
            "The company could not be started because its collection state changed.",
            null,
            []);
    }

    private async Task<OfferOnlinePriceCollectionStartOutcomeDto> RollbackStateChangedAsync(
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction,
        Guid companyPersonId,
        string message,
        IReadOnlyList<OfferOnlinePriceCollectionExclusionDto> exclusions,
        CancellationToken cancellationToken,
        string? companyDisplayName = null)
    {
        companyDisplayName ??= await dbContext.Persons
            .AsNoTracking()
            .Where(person => person.Id == companyPersonId)
            .Select(person => person.DisplayName)
            .SingleOrDefaultAsync(cancellationToken) ?? "Retailer company";
        await transaction.RollbackAsync(cancellationToken);
        dbContext.ChangeTracker.Clear();
        return new OfferOnlinePriceCollectionStartOutcomeDto(
            companyPersonId,
            companyDisplayName,
            "unavailable",
            "stateChanged",
            message,
            null,
            exclusions);
    }

    private async Task<OfferOnlinePriceCollectionCompanyOptionDto> BuildCompanyOptionAsync(
        Offer offer,
        Guid companyPersonId,
        CancellationToken cancellationToken)
    {
        var company = await dbContext.Persons
            .AsNoTracking()
            .SingleOrDefaultAsync(
                person => person.Id == companyPersonId && person.Type == PersonType.Company,
                cancellationToken);
        var comparisons = offer.RetailerComparisons
            .Where(comparison => comparison.Retailer.CompanyPersonId == companyPersonId)
            .OrderBy(comparison => comparison.RetailerDisplayName)
            .ThenBy(comparison => comparison.RetailerId)
            .ToList();
        var products = offer.ProductLines
            .Where(line => line.RequiredQuantity > 0m || line.OptionalQuantity > 0m)
            .OrderBy(line => line.ProductNumberId)
            .ThenBy(line => line.Id)
            .ToList();
        var retailerIds = comparisons.Select(comparison => comparison.RetailerId).ToList();
        var productIds = products.Select(line => line.ProductId).ToList();
        List<RetailerListing> listings = retailerIds.Count == 0 || productIds.Count == 0
            ? []
            : await dbContext.RetailerListings
                .AsNoTracking()
                .Where(listing => retailerIds.Contains(listing.RetailerId)
                    && productIds.Contains(listing.ProductId))
                .ToListAsync(cancellationToken);
        var listingsByPair = listings.ToDictionary(
            listing => (listing.ProductId, listing.RetailerId));
        var exclusions = new List<OfferOnlinePriceCollectionExclusionDto>();
        var eligibleCount = 0;
        foreach (var product in products)
        {
            foreach (var comparison in comparisons)
            {
                var retailer = comparison.Retailer;
                listingsByPair.TryGetValue((product.ProductId, retailer.Id), out var listing);
                var failure = GetEligibilityFailure(retailer, listing);
                if (failure is null)
                {
                    eligibleCount++;
                    continue;
                }

                exclusions.Add(new OfferOnlinePriceCollectionExclusionDto(
                    product.Id,
                    product.ProductId,
                    product.ProductNumberId,
                    product.Title,
                    retailer.Id,
                    comparison.RetailerDisplayName,
                    failure.Value.Code,
                    failure.Value.Message));
            }
        }

        var profileAvailable = await dbContext.RetailerExtractionProfiles.AnyAsync(
            profile => profile.CompanyPersonId == companyPersonId
                && profile.Status == RetailerExtractionProfileStatus.Published
                && profile.IsActive,
            cancellationToken);
        var unfinished = await dbContext.RetailerPriceCollectionRuns
            .AsNoTracking()
            .Include(run => run.CompanyPerson)
            .Include(run => run.ExtractionProfile)
            .Where(run => run.CompanyPersonId == companyPersonId
                && (run.Status == RetailerPriceCollectionRunStatus.Queued
                    || run.Status == RetailerPriceCollectionRunStatus.Running))
            .SingleOrDefaultAsync(cancellationToken);
        var blockingCodes = new List<string>();
        if (comparisons.Count == 0) blockingCodes.Add("noOfferRetailerColumns");
        if (!profileAvailable) blockingCodes.Add("missingActivePublishedProfile");
        if (unfinished is not null) blockingCodes.Add("unfinishedCompanyRun");
        if (eligibleCount == 0) blockingCodes.Add("noCollectableListings");

        return new OfferOnlinePriceCollectionCompanyOptionDto(
            companyPersonId,
            company?.DisplayName ?? "Retailer company",
            comparisons.Select(comparison => new OfferOnlinePriceCollectionRetailerDto(
                comparison.RetailerId,
                comparison.RetailerDisplayName,
                comparison.Retailer.IsActive)).ToList(),
            eligibleCount,
            exclusions,
            profileAvailable,
            unfinished is null
                ? null
                : RetailerPriceCollectionService.ToSummary(
                    unfinished,
                    unfinished.CompanyPerson.DisplayName,
                    unfinished.ExtractionProfile.Version),
            blockingCodes.Count == 0,
            blockingCodes);
    }

    private async Task<List<Guid>> GetEligibleListingIdsAsync(
        Offer offer,
        Guid companyPersonId,
        CancellationToken cancellationToken)
    {
        var productIds = offer.ProductLines
            .Where(line => line.RequiredQuantity > 0m || line.OptionalQuantity > 0m)
            .Select(line => line.ProductId)
            .ToList();
        var retailerIds = offer.RetailerComparisons
            .Where(comparison => comparison.Retailer.CompanyPersonId == companyPersonId
                && comparison.Retailer.IsActive)
            .Select(comparison => comparison.RetailerId)
            .ToList();
        return await dbContext.RetailerListings
            .Where(listing => productIds.Contains(listing.ProductId)
                && retailerIds.Contains(listing.RetailerId)
                && listing.IsActive
                && listing.ProductUrl != null)
            .Select(listing => listing.Id)
            .ToListAsync(cancellationToken);
    }

    private static (string Code, string Message)? GetEligibilityFailure(
        Retailer retailer,
        RetailerListing? listing)
    {
        if (!retailer.IsActive)
        {
            return ("retailerInactive", "The retailer location is inactive.");
        }
        if (listing is null)
        {
            return ("missingListing", "No applicable retailer listing exists.");
        }
        if (!listing.IsActive)
        {
            return ("listingInactive", "The retailer listing is inactive.");
        }
        if (string.IsNullOrWhiteSpace(listing.ProductUrl))
        {
            return ("missingProductUrl", "The retailer listing has no saved product URL.");
        }
        return null;
    }

    private async Task<Offer> LoadOfferAsync(
        Guid siteId,
        Guid offerId,
        bool tracked,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Offers
            .Include(offer => offer.RetailerComparisons)
                .ThenInclude(comparison => comparison.Retailer)
            .Include(offer => offer.ProductLines)
            .AsSplitQuery();
        var offer = await (tracked ? query : query.AsNoTrackingWithIdentityResolution())
            .SingleOrDefaultAsync(
                offer => offer.Id == offerId && offer.SiteId == siteId,
                cancellationToken);
        return offer ?? throw new NotFoundException(nameof(Offer), offerId.ToString());
    }

    private static void EnsureDraft(Offer offer)
    {
        if (offer.Status != OfferStatus.Draft)
        {
            throw new OfferConflictException("Only draft offers can collect online prices.");
        }
    }

    private static string BuildUnavailableMessage(IReadOnlyCollection<string> codes)
    {
        if (codes.Contains("unfinishedCompanyRun"))
        {
            return "Another unfinished price-collection run already exists for this company.";
        }
        if (codes.Contains("missingActivePublishedProfile"))
        {
            return "The company no longer has an active published extraction profile.";
        }
        if (codes.Contains("noCollectableListings"))
        {
            return "The company no longer has any collectable offer listings.";
        }
        return "The company is no longer available for offer price collection.";
    }
}
