using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using FluentValidation;
using MediatR;

namespace Application.Proposals;

/// <summary>Creates the next draft Proposal revision from a finalized Offer.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record CreateProposalCommand(Guid SiteId, Guid OfferId) : IRequest<Guid>;

/// <summary>Updates the only editable metadata on a draft Proposal.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record UpdateProposalMetadataCommand(
    Guid SiteId,
    Guid ProposalId,
    DateOnly? ValidUntil,
    string? PublicNotes,
    string? PaymentTerms) : IRequest;

/// <summary>Issues a draft Proposal and creates its immutable PDF.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record IssueProposalCommand(Guid SiteId, Guid ProposalId) : IRequest;

/// <summary>Loads a Proposal snapshot for Dashboard administration.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record ProposalByIdQuery(Guid SiteId, Guid ProposalId)
    : IRequest<ProposalDetailsDto>;

/// <summary>Loads Proposal revision history for one source Offer.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record ProposalHistoryQuery(Guid SiteId, Guid OfferId)
    : IRequest<IReadOnlyList<ProposalSummaryDto>>;

/// <summary>Loads issued Proposal PDF metadata before granting temporary access.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record ProposalPdfInfoQuery(Guid SiteId, Guid ProposalId)
    : IRequest<ProposalPdfInfoDto>;

public sealed record ProposalPdfDownloadQuery(
    Guid SiteId,
    Guid ProposalId,
    string UserId) : IRequest<ProposalFileResponse>;

public sealed class CreateProposalValidator : AbstractValidator<CreateProposalCommand>
{
    public CreateProposalValidator()
    {
        RuleFor(command => command.SiteId).NotEmpty();
        RuleFor(command => command.OfferId).NotEmpty();
    }
}

public sealed class UpdateProposalMetadataValidator
    : AbstractValidator<UpdateProposalMetadataCommand>
{
    public UpdateProposalMetadataValidator()
    {
        RuleFor(command => command.SiteId).NotEmpty();
        RuleFor(command => command.ProposalId).NotEmpty();
        RuleFor(command => command.PublicNotes)
            .MaximumLength(Domain.Entities.Proposal.MaxPublicNotesLength);
        RuleFor(command => command.PaymentTerms)
            .MaximumLength(Domain.Entities.Proposal.MaxPaymentTermsLength);
    }
}

public sealed class IssueProposalValidator : AbstractValidator<IssueProposalCommand>
{
    public IssueProposalValidator()
    {
        RuleFor(command => command.SiteId).NotEmpty();
        RuleFor(command => command.ProposalId).NotEmpty();
    }
}

public sealed class ProposalRequestHandler(IProposalService proposalService) :
    IRequestHandler<CreateProposalCommand, Guid>,
    IRequestHandler<UpdateProposalMetadataCommand>,
    IRequestHandler<IssueProposalCommand>,
    IRequestHandler<ProposalByIdQuery, ProposalDetailsDto>,
    IRequestHandler<ProposalHistoryQuery, IReadOnlyList<ProposalSummaryDto>>,
    IRequestHandler<ProposalPdfInfoQuery, ProposalPdfInfoDto>,
    IRequestHandler<ProposalPdfDownloadQuery, ProposalFileResponse>
{
    public Task<Guid> Handle(
        CreateProposalCommand request,
        CancellationToken cancellationToken) =>
        proposalService.CreateAsync(request.SiteId, request.OfferId, cancellationToken);

    public Task Handle(
        UpdateProposalMetadataCommand request,
        CancellationToken cancellationToken) =>
        proposalService.UpdateMetadataAsync(request, cancellationToken);

    public Task Handle(
        IssueProposalCommand request,
        CancellationToken cancellationToken) =>
        proposalService.IssueAsync(request.SiteId, request.ProposalId, cancellationToken);

    public Task<ProposalDetailsDto> Handle(
        ProposalByIdQuery request,
        CancellationToken cancellationToken) =>
        proposalService.GetByIdAsync(request.SiteId, request.ProposalId, cancellationToken);

    public Task<IReadOnlyList<ProposalSummaryDto>> Handle(
        ProposalHistoryQuery request,
        CancellationToken cancellationToken) =>
        proposalService.GetHistoryAsync(request.SiteId, request.OfferId, cancellationToken);

    public Task<ProposalPdfInfoDto> Handle(
        ProposalPdfInfoQuery request,
        CancellationToken cancellationToken) =>
        proposalService.GetPdfInfoAsync(request.SiteId, request.ProposalId, cancellationToken);

    public Task<ProposalFileResponse> Handle(
        ProposalPdfDownloadQuery request,
        CancellationToken cancellationToken) =>
        proposalService.DownloadPdfAsync(
            request.SiteId,
            request.ProposalId,
            request.UserId,
            cancellationToken);
}
