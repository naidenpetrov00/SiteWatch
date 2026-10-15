using System.Data;
using Application.RetailerExtractionProfiles;
using Application.SeedWork.Interfaces;
using Domain.Entities;
using Domain.SeedWork.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.RetailerPriceCollections;

public sealed class RetailerPriceCollectionProcessor(
    ApplicationDbContext dbContext,
    IRetailerExtractionEngine extractionEngine,
    ILogger<RetailerPriceCollectionProcessor> logger)
    : IRetailerPriceCollectionProcessor
{
    private static readonly TimeSpan ItemLeaseDuration = TimeSpan.FromMinutes(2);

    public async Task<bool> TryAcquireLeadershipAsync(
        Guid ownerId,
        DateTimeOffset leaseExpiresAt,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var lease = await dbContext.RetailerPriceCollectionWorkerLeases
            .SingleOrDefaultAsync(
                item => item.Id == RetailerPriceCollectionWorkerLease.SingletonId,
                cancellationToken);
        if (lease is null)
        {
            dbContext.RetailerPriceCollectionWorkerLeases.Add(
                new RetailerPriceCollectionWorkerLease
                {
                    OwnerId = ownerId,
                    LeaseExpiresAt = leaseExpiresAt
                });
        }
        else
        {
            if (lease.OwnerId != ownerId && lease.LeaseExpiresAt > now)
            {
                return false;
            }
            lease.OwnerId = ownerId;
            lease.LeaseExpiresAt = leaseExpiresAt;
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }

    public async Task<bool> RenewLeadershipAsync(
        Guid ownerId,
        DateTimeOffset leaseExpiresAt,
        CancellationToken cancellationToken) =>
        await dbContext.RetailerPriceCollectionWorkerLeases
            .Where(lease => lease.Id == RetailerPriceCollectionWorkerLease.SingletonId
                && lease.OwnerId == ownerId
                && lease.LeaseExpiresAt > DateTimeOffset.UtcNow)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    lease => lease.LeaseExpiresAt,
                    leaseExpiresAt),
                cancellationToken) == 1;

    public async Task ReleaseLeadershipAsync(
        Guid ownerId,
        CancellationToken cancellationToken)
    {
        await dbContext.RetailerPriceCollectionWorkerLeases
            .Where(lease => lease.Id == RetailerPriceCollectionWorkerLease.SingletonId
                && lease.OwnerId == ownerId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    lease => lease.LeaseExpiresAt,
                    DateTimeOffset.UtcNow),
                cancellationToken);
    }

    public async Task<bool> TryProcessNextAsync(
        Guid leaderId,
        CancellationToken cancellationToken)
    {
        if (!await HasLeadershipAsync(leaderId, cancellationToken))
        {
            return false;
        }

        var claim = await TryClaimAsync(cancellationToken);
        if (claim is null)
        {
            return false;
        }

        try
        {
            var work = await LoadWorkAsync(claim.ItemId, cancellationToken);
            var ineligible = GetEligibilityFailure(work.CompanyPersonId, work.Listing);
            if (ineligible is not null)
            {
                await CompleteSkippedAsync(
                    claim,
                    ineligible.Value.Code,
                    ineligible.Value.Message,
                    cancellationToken);
                return true;
            }

            var result = await extractionEngine.RunAsync(
                work.CapturedProductUrl,
                work.AllowedHosts,
                work.Rules,
                cancellationToken);
            if (result.Match is null)
            {
                await CompleteFailedAsync(
                    claim,
                    "noRuleMatched",
                    BuildNoMatchDiagnostic(result.Diagnostics),
                    cancellationToken);
                return true;
            }

            await CompleteSucceededAsync(claim, result, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (RetailerExtractionException exception)
        {
            await CompleteFailedAsync(
                claim,
                exception.Kind.ToString().ToLowerInvariant(),
                exception.Message,
                cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Unexpected price extraction failure for run item {RunItemId}.",
                claim.ItemId);
            await CompleteFailedAsync(
                claim,
                "unexpectedFailure",
                "Price extraction could not be completed.",
                cancellationToken);
        }
        return true;
    }

    private Task<bool> HasLeadershipAsync(Guid leaderId, CancellationToken cancellationToken) =>
        dbContext.RetailerPriceCollectionWorkerLeases
            .Where(
                lease => lease.Id == RetailerPriceCollectionWorkerLease.SingletonId
                    && lease.OwnerId == leaderId
                    && lease.LeaseExpiresAt > DateTimeOffset.UtcNow)
            .TagWith("SiteWatch.RetailerPriceCollection.Polling.LeadershipCheck")
            .AnyAsync(cancellationToken);

    private async Task<ClaimedItem?> TryClaimAsync(CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        try
        {
            var now = DateTimeOffset.UtcNow;
            var item = await dbContext.RetailerPriceCollectionRunItems
                .Where(candidate =>
                    (candidate.Status == RetailerPriceCollectionRunItemStatus.Queued
                        || (candidate.Status == RetailerPriceCollectionRunItemStatus.Running
                            && candidate.LeaseExpiresAt <= now))
                    && (candidate.Run.Status == RetailerPriceCollectionRunStatus.Queued
                        || candidate.Run.Status == RetailerPriceCollectionRunStatus.Running))
                .TagWith("SiteWatch.RetailerPriceCollection.Polling.NextItemProbe")
                .OrderBy(candidate => candidate.Run.RequestedAt)
                .ThenBy(candidate => candidate.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (item is null)
            {
                await transaction.CommitAsync(cancellationToken);
                return null;
            }

            var leaseToken = Guid.NewGuid();
            var wasQueued = item.Status == RetailerPriceCollectionRunItemStatus.Queued;
            if (wasQueued)
            {
                item.Claim(leaseToken, now, now.Add(ItemLeaseDuration));
            }
            else
            {
                item.Reclaim(leaseToken, now, now.Add(ItemLeaseDuration));
            }
            await dbContext.SaveChangesAsync(cancellationToken);

            if (wasQueued)
            {
                var updated = await dbContext.RetailerPriceCollectionRuns
                    .Where(run => run.Id == item.RunId
                        && run.QueuedCount > 0
                        && (run.Status == RetailerPriceCollectionRunStatus.Queued
                            || run.Status == RetailerPriceCollectionRunStatus.Running))
                    .ExecuteUpdateAsync(
                        setters => setters
                            .SetProperty(run => run.QueuedCount, run => run.QueuedCount - 1)
                            .SetProperty(run => run.RunningCount, run => run.RunningCount + 1)
                            .SetProperty(run => run.Status, RetailerPriceCollectionRunStatus.Running)
                            .SetProperty(run => run.StartedAt, run => run.StartedAt ?? now)
                            .SetProperty(run => run.LastModified, now)
                            .SetProperty(run => run.LastModifiedBy, run => run.RequestedBy),
                        cancellationToken);
                if (updated != 1)
                {
                    throw new DbUpdateConcurrencyException(
                        "The collection run changed while an item was being claimed.");
                }
            }

            await transaction.CommitAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
            return new ClaimedItem(item.Id, leaseToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            dbContext.ChangeTracker.Clear();
            return null;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            dbContext.ChangeTracker.Clear();
            throw;
        }
    }

    private async Task<WorkSnapshot> LoadWorkAsync(
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var item = await dbContext.RetailerPriceCollectionRunItems
            .AsNoTracking()
            .Include(candidate => candidate.Run)
                .ThenInclude(run => run.ExtractionProfile)
                    .ThenInclude(profile => profile.AllowedHosts)
            .Include(candidate => candidate.Run)
                .ThenInclude(run => run.ExtractionProfile)
                    .ThenInclude(profile => profile.Rules)
            .Include(candidate => candidate.RetailerListing)
                .ThenInclude(listing => listing.Retailer)
            .SingleAsync(candidate => candidate.Id == itemId, cancellationToken);
        return new WorkSnapshot(
            item.Run.CompanyPersonId,
            item.RetailerListing,
            item.CapturedProductUrl!,
            item.Run.ExtractionProfile.AllowedHosts
                .Select(host => host.NormalizedHost)
                .ToHashSet(StringComparer.Ordinal),
            item.Run.ExtractionProfile.Rules
                .Where(rule => rule.IsEnabled)
                .OrderBy(rule => rule.Priority)
                .ToList());
    }

    private static (string Code, string Message)? GetEligibilityFailure(
        Guid companyPersonId,
        RetailerListing listing)
    {
        if (listing.Retailer.CompanyPersonId != companyPersonId)
        {
            return ("companyOwnershipChanged", "The retailer listing no longer belongs to the run company.");
        }
        if (!listing.Retailer.IsActive)
        {
            return ("retailerInactive", "The retailer location became inactive after the run was created.");
        }
        if (!listing.IsActive)
        {
            return ("listingInactive", "The retailer listing became inactive after the run was created.");
        }
        return null;
    }

    private async Task CompleteSucceededAsync(
        ClaimedItem claim,
        RetailerExtractionEngineResult result,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        try
        {
            var item = await LoadOwnedItemAsync(claim, cancellationToken);
            if (item is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return;
            }
            var run = await dbContext.RetailerPriceCollectionRuns
                .Include(candidate => candidate.ExtractionProfile)
                    .ThenInclude(profile => profile.Rules)
                .SingleAsync(candidate => candidate.Id == item.RunId, cancellationToken);
            var listing = await dbContext.RetailerListings
                .Include(candidate => candidate.Retailer)
                .SingleAsync(candidate => candidate.Id == item.RetailerListingId, cancellationToken);
            var eligibilityFailure = GetEligibilityFailure(run.CompanyPersonId, listing);
            if (eligibilityFailure is not null)
            {
                var skippedAt = DateTimeOffset.UtcNow;
                item.SkipRunning(
                    claim.LeaseToken,
                    skippedAt,
                    eligibilityFailure.Value.Code,
                    eligibilityFailure.Value.Message);
                await dbContext.SaveChangesAsync(cancellationToken);
                await UpdateRunCountersAsync(
                    item.RunId,
                    CompletionKind.Skipped,
                    skippedAt,
                    cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                dbContext.ChangeTracker.Clear();
                return;
            }
            var match = result.Match!;
            var matchedRule = run.ExtractionProfile.Rules.Single(
                rule => rule.Id == match.RuleId && rule.IsEnabled);
            var recordedAt = DateTimeOffset.UtcNow;
            var observation = listing.RecordAutomatedPrice(
                match.Amount,
                matchedRule.PriceBasis,
                result.ExtractedAt,
                recordedAt,
                result.FinalUrl,
                run.RequestedBy,
                run.ExtractionProfile,
                matchedRule);
            item.Succeed(claim.LeaseToken, observation, result.FinalUrl, recordedAt);
            dbContext.RetailerPriceObservations.Add(observation);
            await dbContext.SaveChangesAsync(cancellationToken);
            await UpdateRunCountersAsync(
                item.RunId,
                CompletionKind.Succeeded,
                recordedAt,
                cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            dbContext.ChangeTracker.Clear();
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            dbContext.ChangeTracker.Clear();
            throw;
        }
    }

    private Task CompleteFailedAsync(
        ClaimedItem claim,
        string code,
        string message,
        CancellationToken cancellationToken) =>
        CompleteTerminalAsync(claim, CompletionKind.Failed, code, message, cancellationToken);

    private Task CompleteSkippedAsync(
        ClaimedItem claim,
        string code,
        string message,
        CancellationToken cancellationToken) =>
        CompleteTerminalAsync(claim, CompletionKind.Skipped, code, message, cancellationToken);

    private async Task CompleteTerminalAsync(
        ClaimedItem claim,
        CompletionKind completionKind,
        string code,
        string message,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        try
        {
            var item = await LoadOwnedItemAsync(claim, cancellationToken);
            if (item is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return;
            }
            if (completionKind == CompletionKind.Failed)
            {
                var companyPersonId = await dbContext.RetailerPriceCollectionRuns
                    .Where(run => run.Id == item.RunId)
                    .Select(run => run.CompanyPersonId)
                    .SingleAsync(cancellationToken);
                var listing = await dbContext.RetailerListings
                    .Include(candidate => candidate.Retailer)
                    .SingleAsync(
                        candidate => candidate.Id == item.RetailerListingId,
                        cancellationToken);
                var eligibilityFailure = GetEligibilityFailure(companyPersonId, listing);
                if (eligibilityFailure is not null)
                {
                    completionKind = CompletionKind.Skipped;
                    code = eligibilityFailure.Value.Code;
                    message = eligibilityFailure.Value.Message;
                }
            }
            var now = DateTimeOffset.UtcNow;
            if (completionKind == CompletionKind.Failed)
            {
                item.Fail(claim.LeaseToken, now, code, message);
            }
            else
            {
                item.SkipRunning(claim.LeaseToken, now, code, message);
            }
            await dbContext.SaveChangesAsync(cancellationToken);
            await UpdateRunCountersAsync(item.RunId, completionKind, now, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            dbContext.ChangeTracker.Clear();
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            dbContext.ChangeTracker.Clear();
            throw;
        }
    }

    private Task<RetailerPriceCollectionRunItem?> LoadOwnedItemAsync(
        ClaimedItem claim,
        CancellationToken cancellationToken) =>
        dbContext.RetailerPriceCollectionRunItems.SingleOrDefaultAsync(
            item => item.Id == claim.ItemId
                && item.Status == RetailerPriceCollectionRunItemStatus.Running
                && item.LeaseToken == claim.LeaseToken,
            cancellationToken);

    private async Task UpdateRunCountersAsync(
        Guid runId,
        CompletionKind completionKind,
        DateTimeOffset completedAt,
        CancellationToken cancellationToken)
    {
        var succeededDelta = completionKind == CompletionKind.Succeeded ? 1 : 0;
        var failedDelta = completionKind == CompletionKind.Failed ? 1 : 0;
        var skippedDelta = completionKind == CompletionKind.Skipped ? 1 : 0;
        var updated = await dbContext.RetailerPriceCollectionRuns
            .Where(run => run.Id == runId
                && run.RunningCount > 0
                && (run.Status == RetailerPriceCollectionRunStatus.Queued
                    || run.Status == RetailerPriceCollectionRunStatus.Running))
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(run => run.RunningCount, run => run.RunningCount - 1)
                    .SetProperty(run => run.ProcessedCount, run => run.ProcessedCount + 1)
                    .SetProperty(
                        run => run.SucceededCount,
                        run => run.SucceededCount + succeededDelta)
                    .SetProperty(
                        run => run.FailedCount,
                        run => run.FailedCount + failedDelta)
                    .SetProperty(
                        run => run.SkippedCount,
                        run => run.SkippedCount + skippedDelta)
                    .SetProperty(
                        run => run.Status,
                        run => run.QueuedCount == 0 && run.RunningCount == 1
                            ? failedDelta == 1 || run.FailedCount > 0
                                ? RetailerPriceCollectionRunStatus.CompletedWithFailures
                                : RetailerPriceCollectionRunStatus.Completed
                            : RetailerPriceCollectionRunStatus.Running)
                    .SetProperty(
                        run => run.CompletedAt,
                        run => run.QueuedCount == 0 && run.RunningCount == 1
                            ? completedAt
                            : run.CompletedAt)
                    .SetProperty(run => run.LastModified, completedAt)
                    .SetProperty(run => run.LastModifiedBy, run => run.RequestedBy),
                cancellationToken);
        if (updated != 1)
        {
            throw new DbUpdateConcurrencyException(
                "The collection run changed while an item was being completed.");
        }
    }

    private static string BuildNoMatchDiagnostic(
        IReadOnlyList<RetailerExtractionRuleDiagnosticDto> diagnostics)
    {
        var detail = diagnostics.Count == 0
            ? "No enabled extraction rules were available."
            : string.Join(
                " ",
                diagnostics.Select(item =>
                    $"Rule {item.Priority} ({item.RuleName}): {item.Outcome}."));
        var message = $"No enabled extraction rule produced a valid EUR price. {detail}";
        return message.Length <= RetailerPriceCollectionRunItem.MaxDiagnosticMessageLength
            ? message
            : message[..RetailerPriceCollectionRunItem.MaxDiagnosticMessageLength];
    }

    private sealed record ClaimedItem(Guid ItemId, Guid LeaseToken);
    private sealed record WorkSnapshot(
        Guid CompanyPersonId,
        RetailerListing Listing,
        string CapturedProductUrl,
        IReadOnlySet<string> AllowedHosts,
        IReadOnlyList<RetailerExtractionRule> Rules);

    private enum CompletionKind
    {
        Succeeded,
        Failed,
        Skipped
    }
}
