using System.Data;
using Application.RetailerExtractionProfiles;
using Application.SeedWork.Interfaces;
using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.SeedWork.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.RetailerExtractionProfiles.Services;

public sealed class RetailerExtractionProfileService(
    ApplicationDbContext dbContext,
    IUser user,
    IRetailerExtractionTestRunner testRunner) : IRetailerExtractionProfileService
{
    public async Task<IReadOnlyList<RetailerExtractionProfileSummaryDto>> GetVersionsAsync(
        Guid retailerId,
        CancellationToken cancellationToken)
    {
        await EnsureRetailerExistsAsync(retailerId, cancellationToken);
        var profiles = await dbContext.RetailerExtractionProfiles
            .AsNoTracking()
            .Where(profile => profile.RetailerId == retailerId)
            .Include(profile => profile.Rules)
            .OrderByDescending(profile => profile.Version)
            .ToListAsync(cancellationToken);
        return profiles.Select(ToSummaryDto).ToList();
    }

    public async Task<RetailerExtractionCurrentProfilesDto> GetCurrentAsync(
        Guid retailerId,
        CancellationToken cancellationToken)
    {
        await EnsureRetailerExistsAsync(retailerId, cancellationToken);
        var profiles = await dbContext.RetailerExtractionProfiles
            .AsNoTracking()
            .Where(profile => profile.RetailerId == retailerId
                && (profile.Status == RetailerExtractionProfileStatus.Draft
                    || profile.IsActive))
            .Include(profile => profile.AllowedHosts)
            .Include(profile => profile.Rules)
            .ToListAsync(cancellationToken);
        return new RetailerExtractionCurrentProfilesDto(
            profiles.Where(profile => profile.Status == RetailerExtractionProfileStatus.Draft)
                .Select(ToDetailsDto)
                .SingleOrDefault(),
            profiles.Where(profile => profile.IsActive)
                .Select(ToDetailsDto)
                .SingleOrDefault());
    }

    public async Task<RetailerExtractionProfileDetailsDto> GetByIdAsync(
        Guid retailerId,
        Guid profileId,
        CancellationToken cancellationToken) =>
        ToDetailsDto(await LoadProfileAsync(
            retailerId,
            profileId,
            tracking: false,
            cancellationToken));

    public Task<RetailerExtractionProfileDetailsDto> CreateDraftAsync(
        Guid retailerId,
        Guid? sourcePublishedProfileId,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var retailer = await dbContext.Retailers.SingleOrDefaultAsync(
                    item => item.Id == retailerId,
                    cancellationToken);
                Guard.Against.NotFound(retailerId, retailer);

                if (await dbContext.RetailerExtractionProfiles.AnyAsync(
                    profile => profile.RetailerId == retailerId
                        && profile.Status == RetailerExtractionProfileStatus.Draft,
                    cancellationToken))
                {
                    throw new RetailerExtractionProfileConflictException(
                        "Discard or publish the current draft before creating another one.");
                }

                var nextVersion = (await dbContext.RetailerExtractionProfiles
                    .Where(profile => profile.RetailerId == retailerId)
                    .Select(profile => (int?)profile.Version)
                    .MaxAsync(cancellationToken) ?? 0) + 1;

                var sourceProfileId = sourcePublishedProfileId;
                if (!sourceProfileId.HasValue)
                {
                    sourceProfileId = await dbContext.RetailerExtractionProfiles
                        .Where(profile => profile.RetailerId == retailerId && profile.IsActive)
                        .Select(profile => (Guid?)profile.Id)
                        .SingleOrDefaultAsync(cancellationToken);
                }

                RetailerExtractionProfile draft;
                if (sourceProfileId.HasValue)
                {
                    var source = await LoadProfileAsync(
                        retailerId,
                        sourceProfileId.Value,
                        tracking: true,
                        cancellationToken);
                    if (source.Status != RetailerExtractionProfileStatus.Published)
                    {
                        throw new RetailerExtractionProfileConflictException(
                            "Only a published profile can be used as a draft source.");
                    }

                    draft = TryLifecycle(() =>
                        RetailerExtractionProfile.CloneDraft(retailer, source, nextVersion));
                }
                else
                {
                    var host = new Uri(retailer.BaseWebsiteUrl).IdnHost;
                    draft = TryLifecycle(() =>
                        RetailerExtractionProfile.CreateDraft(retailer, nextVersion, [host]));
                }

                SetCreatedAudit(draft);
                dbContext.RetailerExtractionProfiles.Add(draft);
                return draft;
            },
            "The draft could not be created because the retailer profile state changed.",
            cancellationToken);

    public Task DeleteDraftAsync(
        Guid retailerId,
        Guid profileId,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var profile = await LoadProfileAsync(
                    retailerId,
                    profileId,
                    tracking: true,
                    cancellationToken);
                TryLifecycle(profile.EnsureCanDelete);
                dbContext.RetailerExtractionProfiles.Remove(profile);
            },
            "The draft could not be discarded because its lifecycle state changed.",
            cancellationToken);

    public Task<RetailerExtractionProfileDetailsDto> UpdateAllowedHostsAsync(
        Guid retailerId,
        Guid profileId,
        IReadOnlyList<string> allowedHosts,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var profile = await LoadProfileAsync(
                    retailerId,
                    profileId,
                    tracking: true,
                    cancellationToken);
                TryLifecycle(profile.EnsureCanDelete);

                dbContext.RetailerExtractionAllowedHosts.RemoveRange(profile.AllowedHosts);
                await dbContext.SaveChangesAsync(cancellationToken);
                TryLifecycle(() => profile.ReplaceAllowedHosts(allowedHosts));
                SetModifiedAudit(profile);
                return profile;
            },
            "The allowed hosts could not be updated because the draft state changed.",
            cancellationToken);

    public Task<RetailerExtractionProfileDetailsDto> AddRuleAsync(
        Guid retailerId,
        Guid profileId,
        RetailerExtractionRuleRequest request,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var profile = await LoadProfileAsync(
                    retailerId,
                    profileId,
                    tracking: true,
                    cancellationToken);
                TryLifecycle(profile.EnsureCanDelete);
                var rule = TryLifecycle(() => profile.AddRule(ToConfiguration(request)));
                dbContext.RetailerExtractionRules.Add(rule);
                SetModifiedAudit(profile);
                return profile;
            },
            "The extraction rule could not be added.",
            cancellationToken);

    public Task<RetailerExtractionProfileDetailsDto> UpdateRuleAsync(
        Guid retailerId,
        Guid profileId,
        Guid ruleId,
        RetailerExtractionRuleRequest request,
        CancellationToken cancellationToken) =>
        ExecuteProfileMutationAsync(
            retailerId,
            profileId,
            profile =>
            {
                EnsureRuleExists(profile, ruleId);
                profile.UpdateRule(ruleId, ToConfiguration(request));
            },
            "The extraction rule could not be updated.",
            cancellationToken);

    public Task DeleteRuleAsync(
        Guid retailerId,
        Guid profileId,
        Guid ruleId,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var profile = await LoadProfileAsync(
                    retailerId,
                    profileId,
                    tracking: true,
                    cancellationToken);
                TryLifecycle(profile.EnsureCanDelete);
                EnsureRuleExists(profile, ruleId);
                await StageRulePrioritiesAsync(profile, cancellationToken);
                TryLifecycle(() => profile.RemoveRule(ruleId));
                SetModifiedAudit(profile);
            },
            "The extraction rule could not be removed.",
            cancellationToken);

    public Task<RetailerExtractionProfileDetailsDto> SetRuleEnabledAsync(
        Guid retailerId,
        Guid profileId,
        Guid ruleId,
        bool isEnabled,
        CancellationToken cancellationToken) =>
        ExecuteProfileMutationAsync(
            retailerId,
            profileId,
            profile =>
            {
                EnsureRuleExists(profile, ruleId);
                profile.SetRuleEnabled(ruleId, isEnabled);
            },
            "The extraction rule state could not be changed.",
            cancellationToken);

    public Task<RetailerExtractionProfileDetailsDto> ReorderRulesAsync(
        Guid retailerId,
        Guid profileId,
        IReadOnlyList<Guid> orderedRuleIds,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var profile = await LoadProfileAsync(
                    retailerId,
                    profileId,
                    tracking: true,
                    cancellationToken);
                TryLifecycle(profile.EnsureCanDelete);
                await StageRulePrioritiesAsync(profile, cancellationToken);
                TryLifecycle(() => profile.ReorderRules(orderedRuleIds));
                SetModifiedAudit(profile);
                return profile;
            },
            "The extraction rules could not be reordered.",
            cancellationToken);

    public Task<RetailerExtractionProfileDetailsDto> PublishAsync(
        Guid retailerId,
        Guid profileId,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var profile = await LoadProfileAsync(
                    retailerId,
                    profileId,
                    tracking: true,
                    cancellationToken);
                var currentActive = await dbContext.RetailerExtractionProfiles
                    .SingleOrDefaultAsync(
                        item => item.RetailerId == retailerId && item.IsActive,
                        cancellationToken);
                if (currentActive is not null)
                {
                    TryLifecycle(currentActive.Deactivate);
                    SetModifiedAudit(currentActive);
                    await dbContext.SaveChangesAsync(cancellationToken);
                }

                TryLifecycle(() => profile.Publish(DateTimeOffset.UtcNow, GetUserId()));
                SetModifiedAudit(profile);
                return profile;
            },
            "The profile could not be published because its lifecycle state changed.",
            cancellationToken);

    public Task<RetailerExtractionProfileDetailsDto> ActivateAsync(
        Guid retailerId,
        Guid profileId,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var profile = await LoadProfileAsync(
                    retailerId,
                    profileId,
                    tracking: true,
                    cancellationToken);
                if (profile.IsActive)
                {
                    return profile;
                }

                var currentActive = await dbContext.RetailerExtractionProfiles
                    .SingleOrDefaultAsync(
                        item => item.RetailerId == retailerId && item.IsActive,
                        cancellationToken);
                if (currentActive is not null)
                {
                    TryLifecycle(currentActive.Deactivate);
                    SetModifiedAudit(currentActive);
                    await dbContext.SaveChangesAsync(cancellationToken);
                }

                TryLifecycle(profile.Activate);
                SetModifiedAudit(profile);
                return profile;
            },
            "The published profile could not be activated because its lifecycle state changed.",
            cancellationToken);

    public async Task<RetailerExtractionTestResultDto> TestAsync(
        Guid retailerId,
        Guid profileId,
        TestRetailerExtractionProfileRequest request,
        CancellationToken cancellationToken)
    {
        var snapshot = await dbContext.RetailerExtractionProfiles
            .AsNoTracking()
            .Include(profile => profile.AllowedHosts)
            .Include(profile => profile.Rules)
            .SingleOrDefaultAsync(
                profile => profile.Id == profileId && profile.RetailerId == retailerId,
                cancellationToken);
        if (snapshot is null)
        {
            throw new NotFoundException(nameof(RetailerExtractionProfile), profileId.ToString());
        }
        if (snapshot.Status != RetailerExtractionProfileStatus.Draft)
        {
            throw new RetailerExtractionProfileConflictException(
                "Only a draft extraction profile can be tested.");
        }

        string productUrl;
        if (request.SourceType == "retailerListing")
        {
            var listingId = request.RetailerListingId!.Value;
            var listing = await dbContext.RetailerListings
                .AsNoTracking()
                .Where(item => item.Id == listingId
                    && item.RetailerId == retailerId)
                .Select(item => new { item.ProductUrl })
                .SingleOrDefaultAsync(cancellationToken);
            if (listing is null)
            {
                throw new NotFoundException(
                    nameof(RetailerListing),
                    listingId.ToString());
            }
            if (string.IsNullOrWhiteSpace(listing.ProductUrl))
            {
                throw new RetailerExtractionProfileConflictException(
                    "The selected retailer listing does not have a product URL.");
            }

            productUrl = listing.ProductUrl;
        }
        else
        {
            productUrl = request.ManualUrl?.Trim() ?? string.Empty;
        }

        var expectedRevision = snapshot.ConfigurationRevision;
        var runnerResult = await testRunner.RunAsync(
            productUrl,
            snapshot.AllowedHosts
                .Select(host => host.NormalizedHost)
                .ToHashSet(StringComparer.Ordinal),
            snapshot.Rules
                .Where(rule => rule.IsEnabled)
                .OrderBy(rule => rule.Priority)
                .ToList(),
            cancellationToken);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        try
        {
            var current = await dbContext.RetailerExtractionProfiles
                .Include(profile => profile.Rules)
                .SingleOrDefaultAsync(
                    profile => profile.Id == profileId && profile.RetailerId == retailerId,
                    cancellationToken);
            if (current is null)
            {
                throw new NotFoundException(
                    nameof(RetailerExtractionProfile),
                    profileId.ToString());
            }
            if (current.Status != RetailerExtractionProfileStatus.Draft
                || current.ConfigurationRevision != expectedRevision)
            {
                throw new RetailerExtractionProfileConflictException(
                    "The extraction profile changed while the test was running.");
            }

            RetailerExtractionSuccessDto? extraction = null;
            if (runnerResult.Match is not null)
            {
                var matchedRule = current.Rules.SingleOrDefault(
                    rule => rule.Id == runnerResult.Match.RuleId && rule.IsEnabled);
                if (matchedRule is null)
                {
                    throw new RetailerExtractionProfileConflictException(
                        "The extraction profile changed while the test was running.");
                }

                TryLifecycle(() => current.RecordSuccessfulTest(
                    expectedRevision,
                    runnerResult.TestedAt,
                    matchedRule));
                SetModifiedAudit(current);
                await dbContext.SaveChangesAsync(cancellationToken);
                extraction = new RetailerExtractionSuccessDto(
                    runnerResult.Match.Amount,
                    RetailerExtractionRule.EuroCurrencyCode,
                    matchedRule.PriceBasis.ToCode(),
                    current.Id,
                    matchedRule.Id,
                    matchedRule.Name,
                    matchedRule.Priority,
                    runnerResult.Match.RawValue);
            }

            await transaction.CommitAsync(cancellationToken);
            return new RetailerExtractionTestResultDto(
                runnerResult.TestedAt,
                runnerResult.TestedUrl,
                current.Id,
                extraction is not null,
                current.IsCurrentConfigurationValidated,
                extraction,
                runnerResult.Diagnostics);
        }
        catch (DbUpdateException exception)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw new RetailerExtractionProfileConflictException(
                "The extraction profile changed while the test was running.",
                exception);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private Task<RetailerExtractionProfileDetailsDto> ExecuteProfileMutationAsync(
        Guid retailerId,
        Guid profileId,
        Action<RetailerExtractionProfile> mutation,
        string persistenceConflictMessage,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var profile = await LoadProfileAsync(
                    retailerId,
                    profileId,
                    tracking: true,
                    cancellationToken);
                TryLifecycle(profile.EnsureCanDelete);
                TryLifecycle(() => mutation(profile));
                SetModifiedAudit(profile);
                return profile;
            },
            persistenceConflictMessage,
            cancellationToken);

    private async Task<RetailerExtractionProfile> LoadProfileAsync(
        Guid retailerId,
        Guid profileId,
        bool tracking,
        CancellationToken cancellationToken)
    {
        IQueryable<RetailerExtractionProfile> query = dbContext.RetailerExtractionProfiles;
        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var profile = await query
            .Include(item => item.AllowedHosts)
            .Include(item => item.Rules)
            .SingleOrDefaultAsync(
                item => item.Id == profileId,
                cancellationToken);
        Guard.Against.NotFound(profileId, profile);
        if (profile.RetailerId != retailerId)
        {
            throw new RetailerExtractionProfileConflictException(
                "The extraction profile does not belong to the requested retailer.");
        }

        return profile;
    }

    private async Task EnsureRetailerExistsAsync(
        Guid retailerId,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.Retailers.AsNoTracking().AnyAsync(
            retailer => retailer.Id == retailerId,
            cancellationToken))
        {
            throw new NotFoundException(nameof(Retailer), retailerId.ToString());
        }
    }

    private async Task StageRulePrioritiesAsync(
        RetailerExtractionProfile profile,
        CancellationToken cancellationToken)
    {
        var offset = profile.Rules.Count;
        if (offset == 0)
        {
            return;
        }

        await dbContext.RetailerExtractionRules
            .Where(rule => rule.ExtractionProfileId == profile.Id)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    rule => rule.Priority,
                    rule => rule.Priority + offset),
                cancellationToken);

        foreach (var rule in profile.Rules)
        {
            dbContext.Entry(rule).Property(item => item.Priority).OriginalValue =
                rule.Priority + offset;
        }
    }

    private static void EnsureRuleExists(RetailerExtractionProfile profile, Guid ruleId)
    {
        if (profile.Rules.All(rule => rule.Id != ruleId))
        {
            throw new NotFoundException(nameof(RetailerExtractionRule), ruleId.ToString());
        }
    }

    private static RetailerExtractionRuleConfiguration ToConfiguration(
        RetailerExtractionRuleRequest request)
    {
        if (!RetailerExtractionCodes.TryParseRuleType(request.RuleType, out var ruleType)
            || !RetailerExtractionCodes.TryParseDecimalSeparator(
                request.DecimalSeparator,
                out var decimalSeparator)
            || !RetailerExtractionCodes.TryParseThousandsSeparator(
                request.ThousandsSeparator,
                out var thousandsSeparator)
            || !PriceBasisCodes.TryParse(request.PriceBasis, out var priceBasis))
        {
            throw new RetailerExtractionProfileConflictException(
                "The extraction rule contains unsupported configuration values.");
        }

        RetailerExtractionCssValueSource? cssValueSource = null;
        if (!string.IsNullOrWhiteSpace(request.CssValueSource))
        {
            if (!RetailerExtractionCodes.TryParseCssValueSource(
                request.CssValueSource,
                out var parsedSource))
            {
                throw new RetailerExtractionProfileConflictException(
                    "The CSS value source is unsupported.");
            }

            cssValueSource = parsedSource;
        }

        return new RetailerExtractionRuleConfiguration(
            request.Name,
            request.IsEnabled,
            ruleType,
            request.JsonLdObjectType,
            request.JsonLdPricePath,
            request.JsonLdCurrencyPath,
            request.CssSelector,
            cssValueSource,
            request.CssAttributeName,
            decimalSeparator,
            thousandsSeparator,
            request.ExpectedCurrencyCode,
            priceBasis,
            request.MinimumValue,
            request.MaximumValue);
    }

    private async Task ExecuteMutationAsync(
        Func<Task> mutation,
        string persistenceConflictMessage,
        CancellationToken cancellationToken)
    {
        await ExecuteMutationCoreAsync(
            async () =>
            {
                await mutation();
                return true;
            },
            persistenceConflictMessage,
            cancellationToken);
    }

    private async Task<RetailerExtractionProfileDetailsDto> ExecuteMutationAsync(
        Func<Task<RetailerExtractionProfile>> mutation,
        string persistenceConflictMessage,
        CancellationToken cancellationToken)
    {
        var profile = await ExecuteMutationCoreAsync(
            mutation,
            persistenceConflictMessage,
            cancellationToken);
        return ToDetailsDto(profile);
    }

    private async Task<TResult> ExecuteMutationCoreAsync<TResult>(
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
            throw new RetailerExtractionProfileConflictException(
                persistenceConflictMessage,
                exception);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private static void TryLifecycle(Action action)
    {
        try
        {
            action();
        }
        catch (InvalidOperationException exception)
        {
            throw new RetailerExtractionProfileConflictException(exception.Message, exception);
        }
        catch (ArgumentException exception)
        {
            throw new RetailerExtractionProfileConflictException(exception.Message, exception);
        }
        catch (KeyNotFoundException exception)
        {
            throw new RetailerExtractionProfileConflictException(exception.Message, exception);
        }
    }

    private static TResult TryLifecycle<TResult>(Func<TResult> action)
    {
        try
        {
            return action();
        }
        catch (InvalidOperationException exception)
        {
            throw new RetailerExtractionProfileConflictException(exception.Message, exception);
        }
        catch (ArgumentException exception)
        {
            throw new RetailerExtractionProfileConflictException(exception.Message, exception);
        }
        catch (KeyNotFoundException exception)
        {
            throw new RetailerExtractionProfileConflictException(exception.Message, exception);
        }
    }

    private void SetCreatedAudit(RetailerExtractionProfile profile)
    {
        var now = DateTimeOffset.UtcNow;
        var userId = GetUserId();
        profile.Created = now;
        profile.CreatedBy = userId;
        profile.LastModified = now;
        profile.LastModifiedBy = userId;
    }

    private void SetModifiedAudit(RetailerExtractionProfile profile)
    {
        profile.LastModified = DateTimeOffset.UtcNow;
        profile.LastModifiedBy = GetUserId();
    }

    private string GetUserId() => user.Id ?? throw new UnauthorizedAccessException();

    private static RetailerExtractionProfileDetailsDto ToDetailsDto(
        RetailerExtractionProfile profile) =>
        new(
            ToSummaryDto(profile),
            profile.AllowedHosts
                .OrderBy(host => host.NormalizedHost)
                .Select(host => host.NormalizedHost)
                .ToList(),
            profile.Rules
                .OrderBy(rule => rule.Priority)
                .Select(ToRuleDto)
                .ToList());

    private static RetailerExtractionProfileSummaryDto ToSummaryDto(
        RetailerExtractionProfile profile) =>
        new(
            profile.Id,
            profile.RetailerId,
            profile.Version,
            profile.Status.ToCode(),
            profile.IsActive,
            profile.Created,
            profile.LastModified,
            profile.PublishedAt,
            profile.PublishedBy,
            profile.ConfigurationRevision,
            profile.ValidatedConfigurationRevision,
            profile.IsCurrentConfigurationValidated,
            profile.LastSuccessfulTestAt,
            profile.LastSuccessfulTestRuleId,
            profile.Rules.Count,
            profile.Rules.Count(rule => rule.IsEnabled));

    private static RetailerExtractionRuleDto ToRuleDto(RetailerExtractionRule rule) =>
        new(
            rule.Id,
            rule.Name,
            rule.IsEnabled,
            rule.Priority,
            rule.RuleType.ToCode(),
            rule.JsonLdObjectType,
            rule.JsonLdPricePath,
            rule.JsonLdCurrencyPath,
            rule.CssSelector,
            rule.CssValueSource?.ToCode(),
            rule.CssAttributeName,
            rule.DecimalSeparator.ToCode(),
            rule.ThousandsSeparator.ToCode(),
            rule.ExpectedCurrencyCode,
            rule.PriceBasis.ToCode(),
            rule.MinimumValue,
            rule.MaximumValue);
}
