using Application.Proposals;

namespace Application.SeedWork.Interfaces;

public interface IProposalService
{
    Task<Guid> CreateAsync(
        Guid siteId,
        Guid offerId,
        CancellationToken cancellationToken);

    Task UpdateMetadataAsync(
        UpdateProposalMetadataCommand request,
        CancellationToken cancellationToken);

    Task IssueAsync(
        Guid siteId,
        Guid proposalId,
        CancellationToken cancellationToken);

    Task<ProposalDetailsDto> GetByIdAsync(
        Guid siteId,
        Guid proposalId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ProposalSummaryDto>> GetHistoryAsync(
        Guid siteId,
        Guid offerId,
        CancellationToken cancellationToken);

    Task<ProposalPdfInfoDto> GetPdfInfoAsync(
        Guid siteId,
        Guid proposalId,
        CancellationToken cancellationToken);

    Task<ProposalFileResponse> DownloadPdfAsync(
        Guid siteId,
        Guid proposalId,
        string userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ClientProposalSummaryDto>> GetClientListAsync(
        Guid siteId,
        CancellationToken cancellationToken);

    Task<ClientProposalDetailsDto> GetClientByIdAsync(
        Guid siteId,
        Guid proposalId,
        CancellationToken cancellationToken);

    Task<ClientProposalDetailsDto> RespondAsync(
        RespondToProposalCommand request,
        CancellationToken cancellationToken);

    Task<ProposalPdfInfoDto> GetClientPdfInfoAsync(
        Guid siteId,
        Guid proposalId,
        CancellationToken cancellationToken);

    Task<ProposalFileResponse> DownloadClientPdfAsync(
        Guid siteId,
        Guid proposalId,
        string ticketUserId,
        CancellationToken cancellationToken);
}
