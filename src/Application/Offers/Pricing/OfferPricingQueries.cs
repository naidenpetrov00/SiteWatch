using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using FluentValidation;
using MediatR;

namespace Application.Offers.Pricing;

/// <summary>Loads the Product-by-Retailer comparison matrix for an Offer.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record OfferPricingMatrixQuery(Guid SiteId, Guid OfferId)
    : IRequest<OfferPricingMatrixDto>;

public sealed class OfferPricingMatrixValidator : AbstractValidator<OfferPricingMatrixQuery>
{
    public OfferPricingMatrixValidator()
    {
        RuleFor(query => query.SiteId).NotEmpty();
        RuleFor(query => query.OfferId).NotEmpty();
    }
}

public sealed class OfferPricingMatrixHandler(IOfferPricingService service)
    : IRequestHandler<OfferPricingMatrixQuery, OfferPricingMatrixDto>
{
    public Task<OfferPricingMatrixDto> Handle(
        OfferPricingMatrixQuery request,
        CancellationToken cancellationToken) =>
        service.GetMatrixAsync(request.SiteId, request.OfferId, cancellationToken);
}
