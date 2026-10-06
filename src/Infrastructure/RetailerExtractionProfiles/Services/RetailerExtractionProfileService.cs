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
    IRetailerExtractionEngine extractionEngine) : IRetailerExtractionProfileService
{
    public async Task<IReadOnlyList<RetailerExtractionProfileSummaryDto>> GetVersionsAsync(
        Guid companyPersonId,
        CancellationToken cancellationToken)
    {
        await LoadEligibleOwnerAsync(companyPersonId, tracking: false, cancellationToken);
        var profiles = await dbContext.RetailerExtractionProfiles
            .AsNoTracking()
            .Where(profile => profile.CompanyPersonId == companyPersonId)
            .Include(profile => profile.Rules)
            .OrderByDescending(profile => profile.Version)
            .ToListAsync(cancellationToken);
        return profiles.Select(ToSummaryDto).ToList();
    }

    public async Task<RetailerExtractionCurrentProfilesDto> GetCurrentAsync(
        Guid companyPersonId,
        CancellationToken cancellationToken)
    {
        var owner = await LoadEligibleOwnerAsync(
            companyPersonId,
            tracking: false,
            cancellationToken);
        var profiles = await dbContext.RetailerExtractionProfiles
            .AsNoTracking()
            .Where(profile => profile.CompanyPersonId == companyPersonId
                && (profile.Status == RetailerExtractionProfileStatus.Draft
                    || profile.IsActive))
            .Include(profile => profile.AllowedHosts)
            .Include(profile => profile.Rules)
            .ToListAsync(cancellationToken);
        return new RetailerExtractionCurrentProfilesDto(
            ToOwnerDto(owner),
            profiles.Where(profile => profile.Status == RetailerExtractionProfileStatus.Draft)
                .Select(ToDetailsDto)
                .SingleOrDefault(),
            profiles.Where(profile => profile.IsActive)
                .Select(ToDetailsDto)
                .SingleOrDefault());
    }

    public async Task<RetailerExtractionOverviewDto> GetOverviewAsync(
        Guid companyPersonId,
        CancellationToken cancellationToken)
    {
        var owner = await LoadEligibleOwnerAsync(
            companyPersonId,
            tracking: false,
            cancellationToken);
        var active = await dbContext.RetailerExtractionProfiles
            .AsNoTracking()
            .Where(profile => profile.CompanyPersonId == companyPersonId && profile.IsActive)
            .Select(profile => new
            {
                profile.Id,
                profile.Version,
                profile.ConfigurationRevision,
                profile.ValidatedConfigurationRevision,
                profile.LastSuccessfulTestAt,
                profile.LastSuccessfulTestRuleId
            })
            .SingleOrDefaultAsync(cancellationToken);

        return new RetailerExtractionOverviewDto(
            owner.CompanyPerson.Id,
            owner.CompanyPerson.DisplayName,
            owner.Retailers.Count,
            active?.Id,
            active?.Version,
            active is null
                ? null
                : active.ConfigurationRevision > 0
                    && active.ValidatedConfigurationRevision == active.ConfigurationRevision
                    && active.LastSuccessfulTestAt.HasValue
                    && active.LastSuccessfulTestRuleId.HasValue);
    }

    public async Task<RetailerExtractionProfileDetailsDto> GetByIdAsync(
        Guid companyPersonId,
        Guid profileId,
        CancellationToken cancellationToken) =>
        ToDetailsDto(await LoadProfileAsync(
            companyPersonId,
            profileId,
            tracking: false,
            cancellationToken));

    public Task<RetailerExtractionProfileDetailsDto> CreateDraftAsync(
        Guid companyPersonId,
        Guid? sourcePublishedProfileId,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var owner = await LoadEligibleOwnerAsync(
                    companyPersonId,
                    tracking: true,
                    cancellationToken);

                if (await dbContext.RetailerExtractionProfiles.AnyAsync(
                    profile => profile.CompanyPersonId == companyPersonId
                        && profile.Status == RetailerExtractionProfileStatus.Draft,
                    cancellationToken))
                {
                    throw new RetailerExtractionProfileConflictException(
                        "Discard or publish the current draft before creating another one.");
                }

                var nextVersion = (await dbContext.RetailerExtractionProfiles
                    .Where(profile => profile.CompanyPersonId == companyPersonId)
                    .Select(profile => (int?)profile.Version)
                    .MaxAsync(cancellationToken) ?? 0) + 1;

                var sourceProfileId = sourcePublishedProfileId;
                if (!sourceProfileId.HasValue)
                {
                    sourceProfileId = await dbContext.RetailerExtractionProfiles
                        .Where(profile => profile.CompanyPersonId == companyPersonId
                            && profile.IsActive)
                        .Select(profile => (Guid?)profile.Id)
                        .SingleOrDefaultAsync(cancellationToken);
                }

                RetailerExtractionProfile draft;
                if (sourceProfileId.HasValue)
                {
                    var source = await LoadProfileAsync(
                        companyPersonId,
                        sourceProfileId.Value,
                        tracking: true,
                        cancellationToken);
                    if (source.Status != RetailerExtractionProfileStatus.Published)
                    {
                        throw new RetailerExtractionProfileConflictException(
                            "Only a published profile can be used as a draft source.");
                    }

                    draft = TryLifecycle(() =>
                        RetailerExtractionProfile.CloneDraft(
                            owner.CompanyPerson,
                            source,
                            nextVersion));
                }
                else
                {
                    if (nextVersion > 1)
                    {
                        throw new RetailerExtractionProfileConflictException(
                            "A later draft must clone the active or an explicitly selected published profile.");
                    }

                    var hosts = owner.Retailers
                        .Select(retailer => retailer.NormalizedWebsiteHost)
                        .Distinct(StringComparer.Ordinal)
                        .ToList();
                    draft = TryLifecycle(() =>
                        RetailerExtractionProfile.CreateDraft(
                            owner.CompanyPerson,
                            nextVersion,
                            hosts));
                }

                SetCreatedAudit(draft);
                dbContext.RetailerExtractionProfiles.Add(draft);
                return draft;
            },
            "The draft could not be created because the company profile state changed.",
            cancellationToken);

    public Task DeleteDraftAsync(
        Guid companyPersonId,
        Guid profileId,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var profile = await LoadProfileAsync(
                    companyPersonId,
                    profileId,
                    tracking: true,
                    cancellationToken);
                TryLifecycle(profile.EnsureCanDelete);
                dbContext.RetailerExtractionProfiles.Remove(profile);
            },
            "The draft could not be discarded because its lifecycle state changed.",
            cancellationToken);

    public Task<RetailerExtractionProfileDetailsDto> UpdateAllowedHostsAsync(
        Guid companyPersonId,
        Guid profileId,
        IReadOnlyList<string> allowedHosts,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var profile = await LoadProfileAsync(
                    companyPersonId,
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
        Guid companyPersonId,
        Guid profileId,
        RetailerExtractionRuleRequest request,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var profile = await LoadProfileAsync(
                    companyPersonId,
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
        Guid companyPersonId,
        Guid profileId,
        Guid ruleId,
        RetailerExtractionRuleRequest request,
        CancellationToken cancellationToken) =>
        ExecuteProfileMutationAsync(
            companyPersonId,
            profileId,
            profile =>
            {
                EnsureRuleExists(profile, ruleId);
                profile.UpdateRule(ruleId, ToConfiguration(request));
            },
            "The extraction rule could not be updated.",
            cancellationToken);

    public Task DeleteRuleAsync(
        Guid companyPersonId,
        Guid profileId,
        Guid ruleId,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var profile = await LoadProfileAsync(
                    companyPersonId,
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
        Guid companyPersonId,
        Guid profileId,
        Guid ruleId,
        bool isEnabled,
        CancellationToken cancellationToken) =>
        ExecuteProfileMutationAsync(
            companyPersonId,
            profileId,
            profile =>
            {
                EnsureRuleExists(profile, ruleId);
                profile.SetRuleEnabled(ruleId, isEnabled);
            },
            "The extraction rule state could not be changed.",
            cancellationToken);

    public Task<RetailerExtractionProfileDetailsDto> ReorderRulesAsync(
        Guid companyPersonId,
        Guid profileId,
        IReadOnlyList<Guid> orderedRuleIds,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var profile = await LoadProfileAsync(
                    companyPersonId,
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
        Guid companyPersonId,
        Guid profileId,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var profile = await LoadProfileAsync(
                    companyPersonId,
                    profileId,
                    tracking: true,
                    cancellationToken);
                var currentActive = await dbContext.RetailerExtractionProfiles
                    .SingleOrDefaultAsync(
                        item => item.CompanyPersonId == companyPersonId && item.IsActive,
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
        Guid companyPersonId,
        Guid profileId,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var profile = await LoadProfileAsync(
                    companyPersonId,
                    profileId,
                    tracking: true,
                    cancellationToken);
                if (profile.IsActive)
                {
                    return profile;
                }

                var currentActive = await dbContext.RetailerExtractionProfiles
                    .SingleOrDefaultAsync(
                        item => item.CompanyPersonId == companyPersonId && item.IsActive,
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
        Guid companyPersonId,
        Guid profileId,
        TestRetailerExtractionProfileRequest request,
        CancellationToken cancellationToken)
    {
        await LoadEligibleOwnerAsync(companyPersonId, tracking: false, cancellationToken);
        var snapshot = await dbContext.RetailerExtractionProfiles
            .AsNoTracking()
            .Include(profile => profile.AllowedHosts)
            .Include(profile => profile.Rules)
            .SingleOrDefaultAsync(
                profile => profile.Id == profileId
                    && profile.CompanyPersonId == companyPersonId,
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
                    && item.Retailer.CompanyPersonId == companyPersonId)
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
        var runnerResult = await extractionEngine.RunAsync(
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
                    profile => profile.Id == profileId
                        && profile.CompanyPersonId == companyPersonId,
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
                    runnerResult.ExtractedAt,
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
                runnerResult.ExtractedAt,
                runnerResult.FinalUrl,
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
        Guid companyPersonId,
        Guid profileId,
        Action<RetailerExtractionProfile> mutation,
        string persistenceConflictMessage,
        CancellationToken cancellationToken) =>
        ExecuteMutationAsync(
            async () =>
            {
                var profile = await LoadProfileAsync(
                    companyPersonId,
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
        Guid companyPersonId,
        Guid profileId,
        bool tracking,
        CancellationToken cancellationToken)
    {
        await LoadEligibleOwnerAsync(companyPersonId, tracking: false, cancellationToken);
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
        if (profile.CompanyPersonId != companyPersonId)
        {
            throw new RetailerExtractionProfileConflictException(
                "The extraction profile does not belong to the requested company Person.");
        }

        return profile;
    }

    private async Task<EligibleOwner> LoadEligibleOwnerAsync(
        Guid companyPersonId,
        bool tracking,
        CancellationToken cancellationToken)
    {
        IQueryable<Person> personQuery = dbContext.Persons;
        IQueryable<Retailer> retailerQuery = dbContext.Retailers;
        if (!tracking)
        {
            personQuery = personQuery.AsNoTracking();
            retailerQuery = retailerQuery.AsNoTracking();
        }

        var companyPerson = await personQuery.SingleOrDefaultAsync(
            person => person.Id == companyPersonId && person.Type == PersonType.Company,
            cancellationToken);
        if (companyPerson is null)
        {
            throw new NotFoundException(nameof(Person), companyPersonId.ToString());
        }

        var retailers = await retailerQuery
            .Where(retailer => retailer.CompanyPersonId == companyPersonId)
            .OrderBy(retailer => retailer.DisplayName)
            .ThenBy(retailer => retailer.Id)
            .ToListAsync(cancellationToken);
        if (retailers.Count == 0)
        {
            throw new NotFoundException(nameof(Retailer), companyPersonId.ToString());
        }

        return new EligibleOwner(companyPerson, retailers);
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
            profile.CompanyPersonId,
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

    private static RetailerExtractionOwnerDto ToOwnerDto(EligibleOwner owner) =>
        new(
            owner.CompanyPerson.Id,
            owner.CompanyPerson.DisplayName,
            owner.Retailers.Count,
            owner.Retailers
                .Select(retailer => new RetailerExtractionRetailerDto(
                    retailer.Id,
                    retailer.DisplayName,
                    retailer.NormalizedWebsiteHost,
                    retailer.IsActive))
                .ToList());

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

    private sealed record EligibleOwner(
        Person CompanyPerson,
        IReadOnlyList<Retailer> Retailers);
}
