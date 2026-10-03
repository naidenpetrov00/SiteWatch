using System.Data;
using Application.RetailerPriceCollections;
using Application.SeedWork.Interfaces;
using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.SeedWork.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.RetailerPriceCollections;

public sealed class RetailerPriceCollectionService(
    ApplicationDbContext dbContext,
    IUser user) : IRetailerPriceCollectionService
{
    public async Task<RetailerPriceCollectionRunSummaryDto> StartAsync(
        Guid companyPersonId,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        try
        {
            var company = await dbContext.Persons.SingleOrDefaultAsync(
                person => person.Id == companyPersonId && person.Type == PersonType.Company,
                cancellationToken)
                ?? throw new NotFoundException(nameof(Person), companyPersonId.ToString());
            var retailers = await dbContext.Retailers
                .Where(retailer => retailer.CompanyPersonId == companyPersonId)
                .ToListAsync(cancellationToken);
            if (retailers.Count == 0)
            {
                throw new NotFoundException(nameof(Retailer), companyPersonId.ToString());
            }

            if (await dbContext.RetailerPriceCollectionRuns.AnyAsync(
                run => run.CompanyPersonId == companyPersonId
                    && (run.Status == RetailerPriceCollectionRunStatus.Queued
                        || run.Status == RetailerPriceCollectionRunStatus.Running),
                cancellationToken))
            {
                throw new RetailerPriceCollectionConflictException(
                    "Another unfinished price-collection run already exists for this company.");
            }

            var profile = await dbContext.RetailerExtractionProfiles.SingleOrDefaultAsync(
                item => item.CompanyPersonId == companyPersonId
                    && item.Status == RetailerExtractionProfileStatus.Published
                    && item.IsActive,
                cancellationToken);
            if (profile is null)
            {
                throw new RetailerPriceCollectionConflictException(
                    "The company does not have an active published extraction profile.");
            }

            var listings = await dbContext.RetailerListings
                .Include(listing => listing.Retailer)
                .Where(listing => listing.Retailer.CompanyPersonId == companyPersonId)
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
                userId);
            run.Created = now;
            run.CreatedBy = userId;
            run.LastModified = now;
            run.LastModifiedBy = userId;
            dbContext.RetailerPriceCollectionRuns.Add(run);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return ToSummary(run, company.DisplayName, profile.Version);
        }
        catch (DbUpdateException exception)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw new RetailerPriceCollectionConflictException(
                "The price-collection run could not be started because the company state changed.",
                exception);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<IReadOnlyList<RetailerPriceCollectionRunSummaryDto>> GetRecentAsync(
        Guid companyPersonId,
        int limit,
        CancellationToken cancellationToken)
    {
        await EnsureEligibleCompanyAsync(companyPersonId, cancellationToken);
        var runs = await dbContext.RetailerPriceCollectionRuns
            .AsNoTracking()
            .Include(run => run.CompanyPerson)
            .Include(run => run.ExtractionProfile)
            .Where(run => run.CompanyPersonId == companyPersonId)
            .OrderByDescending(run => run.RequestedAt)
            .ThenByDescending(run => run.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);
        return runs.Select(run => ToSummary(
            run,
            run.CompanyPerson.DisplayName,
            run.ExtractionProfile.Version)).ToList();
    }

    public async Task<RetailerPriceCollectionRunDetailsDto> GetByIdAsync(
        Guid companyPersonId,
        Guid runId,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken)
    {
        await EnsureEligibleCompanyAsync(companyPersonId, cancellationToken);
        var run = await dbContext.RetailerPriceCollectionRuns
            .AsNoTracking()
            .Include(item => item.CompanyPerson)
            .Include(item => item.ExtractionProfile)
            .SingleOrDefaultAsync(
                item => item.Id == runId && item.CompanyPersonId == companyPersonId,
                cancellationToken)
            ?? throw new NotFoundException(nameof(RetailerPriceCollectionRun), runId.ToString());

        var itemQuery = dbContext.RetailerPriceCollectionRunItems
            .AsNoTracking()
            .Where(item => item.RunId == runId);
        var totalCount = await itemQuery.CountAsync(cancellationToken);
        var items = await itemQuery
            .Include(item => item.RetailerListing)
                .ThenInclude(listing => listing.Retailer)
            .Include(item => item.RetailerListing)
                .ThenInclude(listing => listing.Product)
            .Include(item => item.RetailerPriceObservation)
                .ThenInclude(observation => observation!.MatchedExtractionRule)
            .OrderBy(item => item.RetailerListing.Retailer.DisplayName)
            .ThenBy(item => item.RetailerListing.Product.NumberId)
            .ThenBy(item => item.Id)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new RetailerPriceCollectionRunDetailsDto(
            ToSummary(run, run.CompanyPerson.DisplayName, run.ExtractionProfile.Version),
            items.Select(ToItem).ToList(),
            pageIndex,
            pageSize,
            totalCount);
    }

    private async Task EnsureEligibleCompanyAsync(
        Guid companyPersonId,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.Persons.AnyAsync(
            person => person.Id == companyPersonId && person.Type == PersonType.Company,
            cancellationToken);
        if (!exists)
        {
            throw new NotFoundException(nameof(Person), companyPersonId.ToString());
        }
        if (!await dbContext.Retailers.AnyAsync(
            retailer => retailer.CompanyPersonId == companyPersonId,
            cancellationToken))
        {
            throw new NotFoundException(nameof(Retailer), companyPersonId.ToString());
        }
    }

    internal static RetailerPriceCollectionRunSummaryDto ToSummary(
        RetailerPriceCollectionRun run,
        string companyDisplayName,
        int profileVersion) =>
        new(
            run.Id,
            run.CompanyPersonId,
            companyDisplayName,
            run.ExtractionProfileId,
            profileVersion,
            run.Status.ToCode(),
            run.RequestedBy,
            run.RequestedAt,
            run.StartedAt,
            run.CompletedAt,
            run.TotalCount,
            run.QueuedCount,
            run.RunningCount,
            run.ProcessedCount,
            run.SucceededCount,
            run.FailedCount,
            run.SkippedCount);

    private static RetailerPriceCollectionRunItemDto ToItem(
        RetailerPriceCollectionRunItem item)
    {
        var listing = item.RetailerListing;
        var observation = item.RetailerPriceObservation;
        var rule = observation?.MatchedExtractionRule;
        return new RetailerPriceCollectionRunItemDto(
            item.Id,
            listing.Id,
            listing.RetailerId,
            listing.Retailer.DisplayName,
            listing.ProductId,
            listing.Product.NumberId,
            listing.Product.Title,
            listing.RetailerProductCode,
            item.Status.ToCode(),
            item.CapturedProductUrl,
            item.FinalSourceUrl,
            item.StartedAt,
            item.CompletedAt,
            observation?.Amount,
            observation?.CurrencyCode,
            observation?.Basis.ToCode(),
            observation?.ObservedAt,
            observation?.RecordedAt,
            rule?.Id,
            rule?.Name,
            rule?.Priority,
            observation?.Id,
            item.DiagnosticCode,
            item.DiagnosticMessage);
    }
}
