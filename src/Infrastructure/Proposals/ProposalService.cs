using System.Data;
using Application.Proposals;
using Application.SeedWork.Exceptions;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.SeedWork.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Proposals;

internal sealed class ProposalService(
    ApplicationDbContext dbContext,
    IUser user,
    IIdentityService identityService,
    ProposalPdfRenderer pdfRenderer,
    ProposalDocumentBlobStorage documentStorage,
    ILogger<ProposalService> logger) : IProposalService
{
    public async Task<Guid> CreateAsync(
        Guid siteId,
        Guid offerId,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var offer = await dbContext.Offers
            .Include(item => item.Site)
            .ThenInclude(site => site.PrimaryClientUser)
            .Include(item => item.Activities)
            .ThenInclude(activity => activity.Sections)
            .Include(item => item.ProductLines)
            .ThenInclude(line => line.PriceSelection)
            .AsSplitQuery()
            .SingleOrDefaultAsync(
                item => item.Id == offerId && item.SiteId == siteId,
                cancellationToken)
            ?? throw new NotFoundException(nameof(Offer), offerId.ToString());

        if (offer.Status != OfferStatus.Finalized)
        {
            throw new ProposalConflictException(
                "A Proposal can only be created from a finalized Offer.");
        }

        var recipient = offer.Site.PrimaryClientUser
            ?? throw new ProposalConflictException(
                "Select a Primary Client Recipient for the Site before creating a Proposal.");
        var existing = await dbContext.Proposals
            .Where(proposal => proposal.SourceOfferId == offerId)
            .OrderBy(proposal => proposal.RevisionNumber)
            .ToListAsync(cancellationToken);
        if (existing.Any(proposal => proposal.Status == ProposalStatus.Draft))
        {
            throw new ProposalConflictException(
                "Issue the existing draft Proposal before creating another revision.");
        }

        var revisionNumber = existing.Count == 0
            ? 1
            : checked(existing[^1].RevisionNumber + 1);
        int? proposalNumber = existing.Count == 0
            ? null
            : existing[0].NumberId;
        Proposal proposal;
        try
        {
            proposal = Proposal.Create(
                offer,
                recipient,
                revisionNumber,
                proposalNumber);
        }
        catch (Exception exception) when (
            exception is InvalidOperationException
                or ArgumentException
                or OverflowException)
        {
            throw new ProposalConflictException(exception.Message, exception);
        }

        SetAuditValues(proposal, isNew: true);
        dbContext.Proposals.Add(proposal);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            throw new ProposalConflictException(
                "The Proposal revision could not be created because its source changed.",
                exception);
        }

        return proposal.Id;
    }

    public async Task UpdateMetadataAsync(
        UpdateProposalMetadataCommand request,
        CancellationToken cancellationToken)
    {
        var proposal = await dbContext.Proposals.SingleOrDefaultAsync(
            item => item.Id == request.ProposalId && item.SiteId == request.SiteId,
            cancellationToken)
            ?? throw new NotFoundException(nameof(Proposal), request.ProposalId.ToString());

        try
        {
            proposal.UpdateMetadata(
                request.ValidUntil,
                request.PublicNotes,
                request.PaymentTerms);
        }
        catch (InvalidOperationException exception)
        {
            throw new ProposalConflictException(exception.Message, exception);
        }

        SetAuditValues(proposal, isNew: false);
        await SaveMutationAsync(
            "The Proposal metadata could not be updated because the revision changed.",
            cancellationToken);
    }

    public async Task IssueAsync(
        Guid siteId,
        Guid proposalId,
        CancellationToken cancellationToken)
    {
        var proposal = await LoadProposalAsync(siteId, proposalId, tracked: true, cancellationToken);
        try
        {
            proposal.EnsureCanIssue();
        }
        catch (InvalidOperationException exception)
        {
            throw new ProposalConflictException(exception.Message, exception);
        }

        var issuedAt = DateTimeOffset.UtcNow;
        var issuedBy = user.Id ?? throw new UnauthorizedAccessException();
        byte[] pdf;
        try
        {
            pdf = pdfRenderer.Render(proposal, issuedAt);
        }
        catch (Exception exception)
        {
            throw new ProposalConflictException(
                "The Proposal PDF could not be generated from its snapshot.",
                exception);
        }

        StoredProposalDocument? stored = null;
        try
        {
            stored = await documentStorage.UploadAsync(
                proposal.Id,
                proposal.NumberId,
                proposal.RevisionNumber,
                pdf,
                issuedAt,
                cancellationToken);
            var document = ProposalDocument.Create(
                proposal,
                stored.BlobName,
                stored.FileName,
                stored.ContentType,
                stored.ContentLength,
                stored.Sha256,
                stored.StoredAt);
            dbContext.ProposalDocuments.Add(document);
            proposal.Issue(issuedAt, issuedBy, document);
            proposal.LastModified = issuedAt;
            proposal.LastModifiedBy = issuedBy;
            await SaveMutationAsync(
                "The Proposal could not be issued because the revision changed.",
                cancellationToken);
        }
        catch
        {
            if (stored is not null)
            {
                try
                {
                    await documentStorage.DeleteIfExistsAsync(
                        stored.BlobName,
                        CancellationToken.None);
                }
                catch (Exception cleanupException)
                {
                    logger.LogWarning(
                        cleanupException,
                        "Unable to remove orphaned Proposal PDF blob {BlobName}.",
                        stored.BlobName);
                }
            }

            throw;
        }
    }

    public async Task<ProposalDetailsDto> GetByIdAsync(
        Guid siteId,
        Guid proposalId,
        CancellationToken cancellationToken) =>
        ProposalDetailsDto.From(
            await LoadProposalAsync(siteId, proposalId, tracked: false, cancellationToken));

    public async Task<IReadOnlyList<ProposalSummaryDto>> GetHistoryAsync(
        Guid siteId,
        Guid offerId,
        CancellationToken cancellationToken)
    {
        var offerExists = await dbContext.Offers.AsNoTracking().AnyAsync(
            offer => offer.Id == offerId && offer.SiteId == siteId,
            cancellationToken);
        if (!offerExists)
        {
            throw new NotFoundException(nameof(Offer), offerId.ToString());
        }

        var proposals = await dbContext.Proposals
            .AsNoTracking()
            .Where(proposal => proposal.SiteId == siteId
                && proposal.SourceOfferId == offerId)
            .OrderByDescending(proposal => proposal.RevisionNumber)
            .ToListAsync(cancellationToken);
        return proposals.Select(ProposalSummaryDto.From).ToList();
    }

    public async Task<ProposalPdfInfoDto> GetPdfInfoAsync(
        Guid siteId,
        Guid proposalId,
        CancellationToken cancellationToken)
    {
        var proposal = await dbContext.Proposals
            .AsNoTracking()
            .Include(item => item.Document)
            .SingleOrDefaultAsync(
                item => item.Id == proposalId && item.SiteId == siteId,
                cancellationToken)
            ?? throw new NotFoundException(nameof(Proposal), proposalId.ToString());
        if (proposal.Status != ProposalStatus.Issued || proposal.Document is null)
        {
            throw new ProposalConflictException(
                "A PDF is available only for an issued Proposal.");
        }

        return new ProposalPdfInfoDto(
            proposal.Document.FileName,
            proposal.Document.ContentType);
    }

    public async Task<ProposalFileResponse> DownloadPdfAsync(
        Guid siteId,
        Guid proposalId,
        string userId,
        CancellationToken cancellationToken)
    {
        if (!await identityService.IsInRoleAsync(userId, UserRoles.Administrator))
        {
            throw new ForbiddenAccessException();
        }

        var proposal = await dbContext.Proposals
            .AsNoTracking()
            .Include(item => item.Document)
            .SingleOrDefaultAsync(
                item => item.Id == proposalId && item.SiteId == siteId,
                cancellationToken)
            ?? throw new NotFoundException(nameof(Proposal), proposalId.ToString());
        if (proposal.Status != ProposalStatus.Issued || proposal.Document is null)
        {
            throw new NotFoundException("Proposal PDF", proposalId.ToString());
        }

        return await documentStorage.DownloadAsync(
            proposal.Document.BlobName,
            proposal.Document.FileName,
            cancellationToken);
    }

    private async Task<Proposal> LoadProposalAsync(
        Guid siteId,
        Guid proposalId,
        bool tracked,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Proposals
            .Include(item => item.Document)
            .Include(item => item.Activities)
            .ThenInclude(activity => activity.Sections)
            .Include(item => item.ProductLines)
            .AsSplitQuery();
        var proposal = await (tracked ? query : query.AsNoTrackingWithIdentityResolution())
            .SingleOrDefaultAsync(
                item => item.Id == proposalId && item.SiteId == siteId,
                cancellationToken);
        return proposal
            ?? throw new NotFoundException(nameof(Proposal), proposalId.ToString());
    }

    private async Task SaveMutationAsync(
        string conflictMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ProposalConflictException(conflictMessage, exception);
        }
        catch (DbUpdateException exception)
        {
            throw new ProposalConflictException(conflictMessage, exception);
        }
    }

    private void SetAuditValues(Proposal proposal, bool isNew)
    {
        var now = DateTimeOffset.UtcNow;
        var userId = user.Id ?? throw new UnauthorizedAccessException();
        if (isNew)
        {
            proposal.Created = now;
            proposal.CreatedBy = userId;
        }

        proposal.LastModified = now;
        proposal.LastModifiedBy = userId;
    }
}
