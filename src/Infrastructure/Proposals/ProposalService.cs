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

        if (existing.Count > 0 && existing[^1].Status == ProposalStatus.Accepted)
        {
            throw new ProposalConflictException(
                "An accepted Proposal is terminal; another revision cannot be created.");
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
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        var proposal = await LoadProposalAsync(siteId, proposalId, tracked: true, cancellationToken);
        await dbContext.Proposals
            .Where(item => item.SourceOfferId == proposal.SourceOfferId)
            .Select(item => item.RevisionNumber)
            .ToListAsync(cancellationToken);
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
            await transaction.CommitAsync(cancellationToken);
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
        CancellationToken cancellationToken)
    {
        var proposal = await LoadProposalAsync(
            siteId,
            proposalId,
            tracked: false,
            cancellationToken);
        var supersededBy = await GetSupersedingRevisionAsync(
            proposal,
            cancellationToken);
        return ProposalDetailsDto.From(proposal, supersededBy);
    }

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
        var highestClientVisibleRevision = proposals
            .Where(IsClientVisible)
            .Select(proposal => (int?)proposal.RevisionNumber)
            .Max();
        return proposals
            .Select(proposal => ProposalSummaryDto.From(
                proposal,
                highestClientVisibleRevision.HasValue
                    && highestClientVisibleRevision.Value > proposal.RevisionNumber
                    ? highestClientVisibleRevision
                    : null))
            .ToList();
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
        if (!IsClientVisible(proposal) || proposal.Document is null)
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
        if (!IsClientVisible(proposal) || proposal.Document is null)
        {
            throw new NotFoundException("Proposal PDF", proposalId.ToString());
        }

        return await documentStorage.DownloadAsync(
            proposal.Document.BlobName,
            proposal.Document.FileName,
            cancellationToken);
    }

    public async Task<IReadOnlyList<ClientProposalSummaryDto>> GetClientListAsync(
        Guid siteId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var hasSiteAccess = await dbContext.Sites
            .AsNoTracking()
            .AnyAsync(site => site.Id == siteId
                && site.Users.Any(siteUser => siteUser.Id == userId),
                cancellationToken);
        if (!hasSiteAccess)
        {
            throw new NotFoundException(nameof(Site), siteId.ToString());
        }

        var proposals = await dbContext.Proposals
            .AsNoTracking()
            .Where(proposal => proposal.SiteId == siteId
                && proposal.RecipientUserId == userId
                && proposal.Site.Users.Any(siteUser => siteUser.Id == userId)
                && (proposal.Status == ProposalStatus.Issued
                    || proposal.Status == ProposalStatus.Accepted
                    || proposal.Status == ProposalStatus.Rejected))
            .OrderByDescending(proposal => proposal.IssuedAt)
            .ThenByDescending(proposal => proposal.RevisionNumber)
            .ToListAsync(cancellationToken);

        var sourceOfferIds = proposals
            .Select(proposal => proposal.SourceOfferId)
            .Distinct()
            .ToList();
        var clientVisibleRevisions = await dbContext.Proposals
            .AsNoTracking()
            .Where(proposal => sourceOfferIds.Contains(proposal.SourceOfferId)
                && (proposal.Status == ProposalStatus.Issued
                    || proposal.Status == ProposalStatus.Accepted
                    || proposal.Status == ProposalStatus.Rejected))
            .Select(proposal => new
            {
                proposal.SourceOfferId,
                proposal.RevisionNumber
            })
            .ToListAsync(cancellationToken);
        var highestByOffer = clientVisibleRevisions
            .GroupBy(proposal => proposal.SourceOfferId)
            .ToDictionary(
                group => group.Key,
                group => group.Max(proposal => proposal.RevisionNumber));
        return proposals.Select(proposal =>
        {
            var highest = highestByOffer[proposal.SourceOfferId];
            return ClientProposalSummaryDto.From(
                proposal,
                highest > proposal.RevisionNumber ? highest : null);
        }).ToList();
    }

    public async Task<ClientProposalDetailsDto> GetClientByIdAsync(
        Guid siteId,
        Guid proposalId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var viewedAt = DateTimeOffset.UtcNow;
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);
        await dbContext.Proposals
            .Where(proposal => proposal.Id == proposalId
                && proposal.SiteId == siteId
                && proposal.RecipientUserId == userId
                && proposal.Site.Users.Any(siteUser => siteUser.Id == userId)
                && (proposal.Status == ProposalStatus.Issued
                    || proposal.Status == ProposalStatus.Accepted
                    || proposal.Status == ProposalStatus.Rejected)
                && proposal.FirstViewedAt == null)
            .ExecuteUpdateAsync(
                updates => updates.SetProperty(
                    proposal => proposal.FirstViewedAt,
                    viewedAt),
                cancellationToken);

        var proposal = await LoadAuthorizedClientProposalAsync(
            siteId,
            proposalId,
            userId,
            tracked: false,
            cancellationToken);
        var supersededBy = await GetSupersedingRevisionAsync(
            proposal,
            cancellationToken);
        var details = ClientProposalDetailsDto.From(proposal, supersededBy);
        await transaction.CommitAsync(cancellationToken);
        return details;
    }

    public async Task<ClientProposalDetailsDto> RespondAsync(
        RespondToProposalCommand request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        var proposal = await LoadAuthorizedClientProposalAsync(
            request.SiteId,
            request.ProposalId,
            userId,
            tracked: true,
            cancellationToken);
        var supersedingRevision = await GetSupersedingRevisionAsync(
            proposal,
            cancellationToken);
        if (supersedingRevision.HasValue)
        {
            throw new ProposalConflictException(
                $"Revision {proposal.RevisionNumber} was superseded by revision {supersedingRevision.Value}.");
        }

        var decision = string.Equals(
            request.Decision,
            nameof(ProposalStatus.Accepted),
            StringComparison.Ordinal)
            ? ProposalStatus.Accepted
            : ProposalStatus.Rejected;
        try
        {
            proposal.Respond(
                decision,
                DateTimeOffset.UtcNow,
                userId,
                request.Comment);
            await SaveMutationAsync(
                "The Proposal response could not be saved because its state changed.",
                cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception exception) when (
            exception is InvalidOperationException or ArgumentException)
        {
            throw new ProposalConflictException(exception.Message, exception);
        }

        return ClientProposalDetailsDto.From(proposal, null);
    }

    public async Task<ProposalPdfInfoDto> GetClientPdfInfoAsync(
        Guid siteId,
        Guid proposalId,
        CancellationToken cancellationToken)
    {
        var proposal = await LoadAuthorizedClientProposalAsync(
            siteId,
            proposalId,
            GetCurrentUserId(),
            tracked: false,
            cancellationToken);
        if (proposal.Document is null)
        {
            throw new NotFoundException("Proposal PDF", proposalId.ToString());
        }

        return new ProposalPdfInfoDto(
            proposal.Document.FileName,
            proposal.Document.ContentType);
    }

    public async Task<ProposalFileResponse> DownloadClientPdfAsync(
        Guid siteId,
        Guid proposalId,
        string ticketUserId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (!string.Equals(userId, ticketUserId, StringComparison.Ordinal))
        {
            throw new NotFoundException("Proposal PDF", proposalId.ToString());
        }

        var proposal = await LoadAuthorizedClientProposalAsync(
            siteId,
            proposalId,
            userId,
            tracked: false,
            cancellationToken);
        if (proposal.Document is null)
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

    private async Task<Proposal> LoadAuthorizedClientProposalAsync(
        Guid siteId,
        Guid proposalId,
        string userId,
        bool tracked,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Proposals
            .Include(proposal => proposal.Document)
            .Include(proposal => proposal.Activities)
            .ThenInclude(activity => activity.Sections)
            .Include(proposal => proposal.ProductLines)
            .Where(proposal => proposal.Id == proposalId
                && proposal.SiteId == siteId
                && proposal.RecipientUserId == userId
                && proposal.Site.Users.Any(siteUser => siteUser.Id == userId)
                && (proposal.Status == ProposalStatus.Issued
                    || proposal.Status == ProposalStatus.Accepted
                    || proposal.Status == ProposalStatus.Rejected))
            .AsSplitQuery();
        var proposal = await (tracked
                ? query
                : query.AsNoTrackingWithIdentityResolution())
            .SingleOrDefaultAsync(cancellationToken);
        return proposal
            ?? throw new NotFoundException(nameof(Proposal), proposalId.ToString());
    }

    private async Task<int?> GetSupersedingRevisionAsync(
        Proposal proposal,
        CancellationToken cancellationToken) =>
        await dbContext.Proposals
            .AsNoTracking()
            .Where(candidate => candidate.SourceOfferId == proposal.SourceOfferId
                && candidate.RevisionNumber > proposal.RevisionNumber
                && (candidate.Status == ProposalStatus.Issued
                    || candidate.Status == ProposalStatus.Accepted
                    || candidate.Status == ProposalStatus.Rejected))
            .Select(candidate => (int?)candidate.RevisionNumber)
            .MaxAsync(cancellationToken);

    private string GetCurrentUserId() =>
        user.Id ?? throw new UnauthorizedAccessException();

    private static bool IsClientVisible(Proposal proposal) =>
        proposal.Status is ProposalStatus.Issued
            or ProposalStatus.Accepted
            or ProposalStatus.Rejected;

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
