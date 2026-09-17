using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using FluentValidation;
using MediatR;

namespace Application.Offers.Queries;

/// <summary>Loads an offer in the context of its owning site.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record OfferByIdQuery(Guid SiteId, Guid OfferId)
    : IRequest<OfferDetailsDto>;

public sealed class OfferByIdValidator : AbstractValidator<OfferByIdQuery>
{
    public OfferByIdValidator()
    {
        RuleFor(query => query.SiteId).NotEmpty();
        RuleFor(query => query.OfferId).NotEmpty();
    }
}

public sealed class OfferByIdHandler(IOfferService offerService)
    : IRequestHandler<OfferByIdQuery, OfferDetailsDto>
{
    public Task<OfferDetailsDto> Handle(
        OfferByIdQuery request,
        CancellationToken cancellationToken) =>
        offerService.GetByIdAsync(
            request.SiteId,
            request.OfferId,
            cancellationToken);
}
