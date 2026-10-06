using System.Data;
using Application.Offers;
using Application.Offers.Commands;
using Application.Offers.Finalization;
using Application.Offers.Queries;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Application.SeedWork.Queries;
using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.SeedWork.Enums;
using FluentValidation;
using FluentValidation.Results;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Offers.Services;

public sealed class OfferService(ApplicationDbContext dbContext, IUser user) : IOfferService
{
    public async Task<Guid> CreateAsync(
        Guid siteId,
        CancellationToken cancellationToken)
    {
        var site = await dbContext.Sites
            .SingleOrDefaultAsync(item => item.Id == siteId, cancellationToken);
        if (site is null)
        {
            throw new NotFoundException(nameof(Site), siteId.ToString());
        }

        var offer = Offer.Create(site);
        SetAuditValues(offer, isNew: true);
        dbContext.Offers.Add(offer);
        await dbContext.SaveChangesAsync(cancellationToken);

        return offer.Id;
    }

    public Task UpdateMetadataAsync(
        UpdateOfferMetadataCommand request,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var offer = await GetTrackedOfferAsync(
                    request.SiteId,
                    request.OfferId,
                    cancellationToken);
                EnsureDraft(offer);
                offer.UpdateMetadata(request.Title, request.Notes);
                SetAuditValues(offer, isNew: false);
            },
            "The metadata could not be updated because the Offer changed.",
            cancellationToken);

    public Task<Guid> AddActivityAsync(
        AddOfferActivityCommand request,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var offer = await GetTrackedOfferGraphAsync(
                    request.SiteId,
                    request.OfferId,
                    cancellationToken);
                EnsureDraft(offer);
                if (offer.Activities.Any(
                        selected => selected.SourceActivityId == request.ActivityId))
                {
                    throw new OfferConflictException(
                        "The activity is already included in this offer.");
                }

                var activity = await dbContext.ActivityCatalogNodes
                    .OfType<Activity>()
                    .Include(current => current.RequirementSections)
                    .ThenInclude(section => section.ProductRequirements)
                    .ThenInclude(requirement => requirement.Product)
                    .SingleOrDefaultAsync(
                        current => current.Id == request.ActivityId,
                        cancellationToken);
                if (activity is null)
                {
                    throw new NotFoundException(nameof(Activity), request.ActivityId.ToString());
                }

                if (activity.Status != ActivityStatus.Active)
                {
                    throw new OfferConflictException(
                        "Only active activities can be added to an offer.");
                }

                var measurements = ValidateSourceMeasurements(
                    activity.RequirementSections,
                    request.SectionMeasurements);
                var sortOrder = offer.Activities.Count == 0
                    ? 0
                    : offer.Activities.Max(selected => selected.SortOrder) + 1;
                var offerActivity = OfferActivity.Create(offer, activity, sortOrder);
                offer.AddActivity(offerActivity);
                dbContext.OfferActivities.Add(offerActivity);

                var linesByProductId = offer.ProductLines.ToDictionary(line => line.ProductId);
                foreach (var sourceSection in activity.RequirementSections
                             .OrderBy(section => section.SortOrder)
                             .ThenBy(section => section.Id))
                {
                    var offerSection = offerActivity.AddSection(
                        sourceSection,
                        measurements[sourceSection.Id]);
                    foreach (var sourceRequirement in sourceSection.ProductRequirements
                                 .OrderBy(requirement => requirement.SortOrder)
                                 .ThenBy(requirement => requirement.Id))
                    {
                        if (!linesByProductId.TryGetValue(
                                sourceRequirement.ProductId,
                                out var productLine))
                        {
                            productLine = OfferProductLine.Create(
                                offer,
                                sourceRequirement.Product);
                            offer.AddProductLine(productLine);
                            dbContext.OfferProductLines.Add(productLine);
                            linesByProductId.Add(productLine.ProductId, productLine);
                        }

                        productLine.AddContribution(offerSection, sourceRequirement);
                    }
                }

                SetAuditValues(offer, isNew: false);
                return offerActivity.Id;
            },
            "The activity could not be added because the Offer or catalog changed.",
            cancellationToken);

    public Task RemoveActivityAsync(
        Guid siteId,
        Guid offerId,
        Guid offerActivityId,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var offer = await GetTrackedOfferGraphAsync(
                    siteId,
                    offerId,
                    cancellationToken);
                EnsureDraft(offer);
                var activity = offer.Activities.SingleOrDefault(
                    current => current.Id == offerActivityId);
                if (activity is null)
                {
                    throw new NotFoundException(
                        nameof(OfferActivity),
                        offerActivityId.ToString());
                }

                var affectedLines = activity.Sections
                    .SelectMany(section => section.ProductContributions)
                    .Select(contribution => contribution.OfferProductLine)
                    .DistinctBy(line => line.Id)
                    .ToList();
                foreach (var contribution in activity.Sections
                             .SelectMany(section => section.ProductContributions)
                             .ToList())
                {
                    contribution.OfferProductLine.RemoveContribution(contribution);
                }

                offer.RemoveActivity(activity);
                dbContext.OfferActivities.Remove(activity);
                foreach (var line in affectedLines)
                {
                    if (line.Contributions.Count == 0)
                    {
                        offer.RemoveProductLine(line);
                        dbContext.OfferProductLines.Remove(line);
                    }
                    else
                    {
                        line.RecalculateTotals();
                    }
                }

                SetAuditValues(offer, isNew: false);
            },
            "The activity could not be removed because the Offer changed.",
            cancellationToken);

    public Task UpdateActivityMeasurementsAsync(
        UpdateOfferActivityMeasurementsCommand request,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var offer = await GetTrackedOfferGraphAsync(
                    request.SiteId,
                    request.OfferId,
                    cancellationToken);
                EnsureDraft(offer);
                var activity = offer.Activities.SingleOrDefault(
                    current => current.Id == request.OfferActivityId);
                if (activity is null)
                {
                    throw new NotFoundException(
                        nameof(OfferActivity),
                        request.OfferActivityId.ToString());
                }

                var measurements = ValidateOfferMeasurements(
                    activity.Sections,
                    request.SectionMeasurements);
                var affectedLines = activity.Sections
                    .SelectMany(section => section.ProductContributions)
                    .Select(contribution => contribution.OfferProductLine)
                    .DistinctBy(line => line.Id)
                    .ToList();
                offer.UpdateActivityMeasurements(activity, measurements);

                foreach (var line in affectedLines)
                {
                    line.RecalculateTotals();
                }

                SetAuditValues(offer, isNew: false);
            },
            "The measurements could not be updated because the Offer changed.",
            cancellationToken);

    public Task ArchiveAsync(
        Guid siteId,
        Guid offerId,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var offer = await GetTrackedOfferAsync(siteId, offerId, cancellationToken);
                if (offer.Archive())
                {
                    SetAuditValues(offer, isNew: false);
                }
            },
            "The Offer could not be archived because it changed.",
            cancellationToken);

    public async Task<OfferFinalizationReadinessDto> GetReadinessAsync(
        Guid siteId,
        Guid offerId,
        CancellationToken cancellationToken)
    {
        var offer = await LoadFinalizationOfferAsync(
            siteId,
            offerId,
            tracked: false,
            cancellationToken: cancellationToken);
        return await EvaluateReadinessAsync(offer, cancellationToken);
    }

    public Task FinalizeAsync(
        Guid siteId,
        Guid offerId,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var offer = await LoadFinalizationOfferAsync(
                    siteId,
                    offerId,
                    tracked: true,
                    cancellationToken: cancellationToken);
                var readiness = await EvaluateReadinessAsync(offer, cancellationToken);
                if (!readiness.CanFinalize)
                {
                    throw new OfferConflictException(
                        string.Join(" ", readiness.BlockingReasons));
                }

                var now = DateTimeOffset.UtcNow;
                var userId = user.Id ?? throw new UnauthorizedAccessException();
                offer.Finalize(now, userId);
                offer.LastModified = now;
                offer.LastModifiedBy = userId;
            },
            "The Offer could not be finalized because it changed.",
            cancellationToken);

    public async Task<OfferDetailsDto> GetByIdAsync(
        Guid siteId,
        Guid offerId,
        CancellationToken cancellationToken)
    {
        var offer = await dbContext.Offers
            .AsNoTrackingWithIdentityResolution()
            .Include(item => item.Site)
            .Include(item => item.Activities)
            .ThenInclude(activity => activity.Sections)
            .ThenInclude(section => section.ProductContributions)
            .Include(item => item.ProductLines)
            .ThenInclude(line => line.Contributions)
            .AsSplitQuery()
            .SingleOrDefaultAsync(
                item => item.Id == offerId && item.SiteId == siteId,
                cancellationToken);
        if (offer is null)
        {
            throw new NotFoundException(nameof(Offer), offerId.ToString());
        }

        return OfferDetailsDto.From(offer);
    }

    public async Task<IReadOnlyList<OfferActivityCatalogNodeDto>> GetActivityCatalogAsync(
        OfferActivityCatalogQuery request,
        CancellationToken cancellationToken)
    {
        var selectedActivityIds = await dbContext.Offers
            .AsNoTracking()
            .Where(offer => offer.Id == request.OfferId && offer.SiteId == request.SiteId)
            .SelectMany(offer => offer.Activities)
            .Select(activity => activity.SourceActivityId)
            .ToListAsync(cancellationToken);
        if (!await OfferExistsAsync(request.SiteId, request.OfferId, cancellationToken))
        {
            throw new NotFoundException(nameof(Offer), request.OfferId.ToString());
        }

        var nodes = await dbContext.ActivityCatalogNodes
            .AsNoTracking()
            .OrderBy(node => node.ParentFolderId)
            .ThenBy(node => node.SortOrder)
            .ThenBy(node => node.Name)
            .ToListAsync(cancellationToken);
        var normalizedSearch = NormalizeSearch(request.SearchTerm);
        if (normalizedSearch.Length > 0)
        {
            var includedIds = SelectSearchNodes(nodes, normalizedSearch);
            nodes = nodes.Where(node => includedIds.Contains(node.Id)).ToList();
        }

        var selected = selectedActivityIds.ToHashSet();
        return nodes.Select(node => node switch
            {
                Activity activity => new OfferActivityCatalogNodeDto(
                    activity.Id,
                    "activity",
                    activity.ParentFolderId,
                    activity.Name,
                    activity.SortOrder,
                    activity.NumberId,
                    activity.Status.ToString(),
                    selected.Contains(activity.Id)),
                _ => new OfferActivityCatalogNodeDto(
                    node.Id,
                    "folder",
                    node.ParentFolderId,
                    node.Name,
                    node.SortOrder,
                    null,
                    null,
                    false)
            })
            .ToList();
    }

    public async Task<OfferActivityCandidateDto> GetActivityCandidateAsync(
        Guid siteId,
        Guid offerId,
        Guid activityId,
        CancellationToken cancellationToken)
    {
        var offer = await dbContext.Offers
            .AsNoTracking()
            .Include(current => current.Activities)
            .SingleOrDefaultAsync(
                current => current.Id == offerId && current.SiteId == siteId,
                cancellationToken);
        if (offer is null)
        {
            throw new NotFoundException(nameof(Offer), offerId.ToString());
        }

        EnsureDraft(offer);
        if (offer.Activities.Any(selected => selected.SourceActivityId == activityId))
        {
            throw new OfferConflictException(
                "The activity is already included in this offer.");
        }

        var activity = await dbContext.ActivityCatalogNodes
            .OfType<Activity>()
            .AsNoTracking()
            .Include(current => current.RequirementSections)
            .SingleOrDefaultAsync(current => current.Id == activityId, cancellationToken);
        if (activity is null)
        {
            throw new NotFoundException(nameof(Activity), activityId.ToString());
        }

        if (activity.Status != ActivityStatus.Active)
        {
            throw new OfferConflictException(
                "Only active activities can be added to an offer.");
        }

        return new OfferActivityCandidateDto(
            activity.Id,
            activity.NumberId,
            activity.Name,
            activity.Description,
            activity.RequirementSections
                .OrderBy(section => section.SortOrder)
                .ThenBy(section => section.Id)
                .Select(section => new OfferActivityCandidateSectionDto(
                    section.Id,
                    section.Name,
                    section.BasisQuantity,
                    section.MeasurementUnit.ToCode(),
                    section.SortOrder))
                .ToList());
    }

    public async Task<PagedResult<OfferSummaryDto>> GetBySiteAsync(
        SiteOffersQuery request,
        CancellationToken cancellationToken)
    {
        var siteExists = await dbContext.Sites
            .AsNoTracking()
            .AnyAsync(site => site.Id == request.SiteId, cancellationToken);
        if (!siteExists)
        {
            throw new NotFoundException(nameof(Site), request.SiteId.ToString());
        }

        var result = await dbContext.Offers
            .AsNoTracking()
            .Where(offer => offer.SiteId == request.SiteId)
            .ToPagedResultAsync<Offer, Offer, SiteOffersQuery>(
                request,
                SiteOffersQuery.Table,
                query => query,
                cancellationToken);

        return new PagedResult<OfferSummaryDto>(
            result.Items.Select(OfferSummaryDto.From).ToList(),
            result.FilteredCount,
            result.TotalCount);
    }

    private async Task<Offer> LoadFinalizationOfferAsync(
        Guid siteId,
        Guid offerId,
        bool tracked,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Offers
            .Include(offer => offer.Activities)
            .Include(offer => offer.ProductLines)
            .ThenInclude(line => line.PriceSelection)
            .AsSplitQuery();
        var offer = await (tracked
                ? query
                : query.AsNoTrackingWithIdentityResolution())
            .SingleOrDefaultAsync(
                item => item.Id == offerId && item.SiteId == siteId,
                cancellationToken);
        return offer ?? throw new NotFoundException(nameof(Offer), offerId.ToString());
    }

    private async Task<OfferFinalizationReadinessDto> EvaluateReadinessAsync(
        Offer offer,
        CancellationToken cancellationToken)
    {
        var selections = offer.ProductLines
            .Where(line => line.PriceSelection is not null)
            .Select(line => new
            {
                LineId = line.Id,
                ListingId = line.PriceSelection!.RetailerListingId,
                ObservationId = line.PriceSelection!.RetailerPriceObservationId
            })
            .ToList();
        var listingIds = selections.Select(item => item.ListingId).Distinct().ToList();
        var latest = await dbContext.RetailerListings
            .AsNoTracking()
            .Where(listing => listingIds.Contains(listing.Id))
            .Select(listing => new
            {
                ListingId = listing.Id,
                LatestObservationId = listing.PriceObservations
                    .OrderByDescending(observation => observation.ObservedAt)
                    .ThenByDescending(observation => observation.RecordedAt)
                    .ThenByDescending(observation => observation.Id)
                    .Select(observation => observation.Id)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);
        var latestByListingId = latest.ToDictionary(
            item => item.ListingId,
            item => item.LatestObservationId);
        var linesWithNewerObservations = selections
            .Where(item => latestByListingId.TryGetValue(item.ListingId, out var latestId)
                && latestId != Guid.Empty
                && latestId != item.ObservationId)
            .Select(item => item.LineId)
            .ToHashSet();
        var unfinishedOnlinePriceCollectionRunCount = await dbContext
            .RetailerPriceCollectionRuns
            .CountAsync(
                run => run.OfferId == offer.Id
                    && (run.Status == RetailerPriceCollectionRunStatus.Queued
                        || run.Status == RetailerPriceCollectionRunStatus.Running),
                cancellationToken);
        return OfferFinalizationReadiness.Evaluate(
            offer,
            linesWithNewerObservations,
            unfinishedOnlinePriceCollectionRunCount);
    }

    private async Task<Offer> GetTrackedOfferAsync(
        Guid siteId,
        Guid offerId,
        CancellationToken cancellationToken)
    {
        var offer = await dbContext.Offers
            .SingleOrDefaultAsync(
                item => item.Id == offerId && item.SiteId == siteId,
                cancellationToken);

        return offer ?? throw new NotFoundException(nameof(Offer), offerId.ToString());
    }

    private async Task<Offer> GetTrackedOfferGraphAsync(
        Guid siteId,
        Guid offerId,
        CancellationToken cancellationToken)
    {
        var offer = await dbContext.Offers
            .Include(current => current.Activities)
            .ThenInclude(activity => activity.Sections)
            .ThenInclude(section => section.ProductContributions)
            .Include(current => current.ProductLines)
            .ThenInclude(line => line.Contributions)
            .AsSplitQuery()
            .SingleOrDefaultAsync(
                current => current.Id == offerId && current.SiteId == siteId,
                cancellationToken);

        return offer ?? throw new NotFoundException(nameof(Offer), offerId.ToString());
    }

    private Task<bool> OfferExistsAsync(
        Guid siteId,
        Guid offerId,
        CancellationToken cancellationToken) =>
        dbContext.Offers.AsNoTracking().AnyAsync(
            offer => offer.Id == offerId && offer.SiteId == siteId,
            cancellationToken);

    private static Dictionary<Guid, decimal> ValidateSourceMeasurements(
        IReadOnlyCollection<ActivityRequirementSection> sections,
        IReadOnlyList<OfferSectionMeasurementInput> inputs)
    {
        var measurements = inputs.ToDictionary(input => input.SectionId, input => input.RequestedMeasurement);
        if (measurements.Count != sections.Count
            || sections.Any(section => !measurements.ContainsKey(section.Id)))
        {
            ThrowMeasurementValidation(
                "Supply exactly one requested measurement for every current activity section.");
        }

        return measurements;
    }

    private static Dictionary<Guid, decimal> ValidateOfferMeasurements(
        IReadOnlyCollection<OfferActivitySection> sections,
        IReadOnlyList<OfferSectionMeasurementInput> inputs)
    {
        var measurements = inputs.ToDictionary(input => input.SectionId, input => input.RequestedMeasurement);
        if (measurements.Count != sections.Count
            || sections.Any(section => !measurements.ContainsKey(section.Id)))
        {
            ThrowMeasurementValidation(
                "Supply exactly one requested measurement for every selected activity section.");
        }

        return measurements;
    }

    private static void ThrowMeasurementValidation(string message) =>
        throw new ValidationException(
            [new ValidationFailure("SectionMeasurements", message)]);

    private static void EnsureDraft(Offer offer)
    {
        if (offer.Status != OfferStatus.Draft)
        {
            throw new OfferConflictException("Only draft offers can be edited.");
        }
    }

    private static string NormalizeSearch(string? value) =>
        string.Join(
                " ",
                (value ?? string.Empty)
                    .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            .ToUpperInvariant();

    private static HashSet<Guid> SelectSearchNodes(
        IReadOnlyCollection<ActivityCatalogNode> nodes,
        string normalizedSearch)
    {
        var nodesById = nodes.ToDictionary(node => node.Id);
        var included = new HashSet<Guid>();
        foreach (var activity in nodes.OfType<Activity>().Where(activity =>
                     activity.NormalizedName.Contains(normalizedSearch)
                     || activity.NumberId.ToString().Contains(normalizedSearch)))
        {
            included.Add(activity.Id);
            var parentId = activity.ParentFolderId;
            var visited = new HashSet<Guid>();
            while (parentId.HasValue
                   && visited.Add(parentId.Value)
                   && nodesById.TryGetValue(parentId.Value, out var parent))
            {
                included.Add(parent.Id);
                parentId = parent.ParentFolderId;
            }
        }

        return included;
    }

    private async Task ExecuteMutationAsync(
        Func<Task> mutation,
        string conflictMessage,
        CancellationToken cancellationToken)
    {
        await ExecuteMutationAsync(
            async () =>
            {
                await mutation();
                return true;
            },
            conflictMessage,
            cancellationToken);
    }

    private async Task<TResult> ExecuteMutationAsync<TResult>(
        Func<Task<TResult>> mutation,
        string conflictMessage,
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
            throw new OfferConflictException(conflictMessage, exception);
        }
        catch (SqlException exception) when (exception.Number is 1205 or 1222)
        {
            throw new OfferConflictException(conflictMessage, exception);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private void SetAuditValues(Offer offer, bool isNew)
    {
        var now = DateTimeOffset.UtcNow;
        var userId = user.Id ?? throw new UnauthorizedAccessException();
        if (isNew)
        {
            offer.Created = now;
            offer.CreatedBy = userId;
        }

        offer.LastModified = now;
        offer.LastModifiedBy = userId;
    }
}
