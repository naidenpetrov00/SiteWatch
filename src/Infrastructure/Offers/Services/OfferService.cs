using Application.Offers;
using Application.Offers.Commands;
using Application.Offers.Queries;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Application.SeedWork.Queries;
using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.SeedWork.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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

    public async Task UpdateMetadataAsync(
        UpdateOfferMetadataCommand request,
        CancellationToken cancellationToken)
    {
        var offer = await GetTrackedOfferAsync(
            request.SiteId,
            request.OfferId,
            cancellationToken);
        if (offer.Status != OfferStatus.Draft)
        {
            throw new OfferConflictException("Only draft offers can be edited.");
        }

        offer.UpdateMetadata(request.Title, request.Notes);
        SetAuditValues(offer, isNew: false);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ArchiveAsync(
        Guid siteId,
        Guid offerId,
        CancellationToken cancellationToken)
    {
        var offer = await GetTrackedOfferAsync(siteId, offerId, cancellationToken);
        if (!offer.Archive())
        {
            return;
        }

        SetAuditValues(offer, isNew: false);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<OfferDetailsDto> GetByIdAsync(
        Guid siteId,
        Guid offerId,
        CancellationToken cancellationToken)
    {
        var offer = await dbContext.Offers
            .AsNoTracking()
            .Include(item => item.Site)
            .SingleOrDefaultAsync(
                item => item.Id == offerId && item.SiteId == siteId,
                cancellationToken);
        if (offer is null)
        {
            throw new NotFoundException(nameof(Offer), offerId.ToString());
        }

        return OfferDetailsDto.From(offer);
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
