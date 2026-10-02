using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using FluentValidation;
using MediatR;

namespace Application.Offers.Finalization;

/// <summary>Loads server-calculated readiness for an Offer.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record OfferFinalizationReadinessQuery(Guid SiteId, Guid OfferId)
    : IRequest<OfferFinalizationReadinessDto>;

/// <summary>Permanently finalizes a ready draft Offer.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record FinalizeOfferCommand(Guid SiteId, Guid OfferId) : IRequest;

public sealed class OfferFinalizationReadinessValidator
    : AbstractValidator<OfferFinalizationReadinessQuery>
{
    public OfferFinalizationReadinessValidator()
    {
        RuleFor(query => query.SiteId).NotEmpty();
        RuleFor(query => query.OfferId).NotEmpty();
    }
}

public sealed class FinalizeOfferValidator : AbstractValidator<FinalizeOfferCommand>
{
    public FinalizeOfferValidator()
    {
        RuleFor(command => command.SiteId).NotEmpty();
        RuleFor(command => command.OfferId).NotEmpty();
    }
}

public sealed class OfferFinalizationReadinessHandler(IOfferService offerService)
    : IRequestHandler<OfferFinalizationReadinessQuery, OfferFinalizationReadinessDto>
{
    public Task<OfferFinalizationReadinessDto> Handle(
        OfferFinalizationReadinessQuery query,
        CancellationToken cancellationToken) =>
        offerService.GetReadinessAsync(query.SiteId, query.OfferId, cancellationToken);
}

public sealed class FinalizeOfferHandler(IOfferService offerService)
    : IRequestHandler<FinalizeOfferCommand>
{
    public Task Handle(
        FinalizeOfferCommand command,
        CancellationToken cancellationToken) =>
        offerService.FinalizeAsync(command.SiteId, command.OfferId, cancellationToken);
}
