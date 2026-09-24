using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using FluentValidation;
using MediatR;

namespace Application.Offers.Pricing;

/// <summary>Loads the Product-by-Retailer comparison matrix for an Offer.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record OfferPricingMatrixQuery(Guid SiteId, Guid OfferId)
    : IRequest<OfferPricingMatrixDto>;

/// <summary>Loads newest-first price history for one Product–Retailer listing.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record RetailerPriceHistoryQuery : IRequest<RetailerPriceHistoryDto>
{
    public Guid ProductId { get; set; }
    public Guid RetailerId { get; set; }
    public int PageIndex { get; init; }
    public int PageSize { get; init; } = 25;
}

public sealed class OfferPricingMatrixValidator : AbstractValidator<OfferPricingMatrixQuery>
{
    public OfferPricingMatrixValidator()
    {
        RuleFor(query => query.SiteId).NotEmpty();
        RuleFor(query => query.OfferId).NotEmpty();
    }
}

public sealed class RetailerPriceHistoryValidator
    : AbstractValidator<RetailerPriceHistoryQuery>
{
    public RetailerPriceHistoryValidator()
    {
        RuleFor(query => query.ProductId).NotEmpty();
        RuleFor(query => query.RetailerId).NotEmpty();
        RuleFor(query => query.PageIndex).GreaterThanOrEqualTo(0);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
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

public sealed class RetailerPriceHistoryHandler(IOfferPricingService service)
    : IRequestHandler<RetailerPriceHistoryQuery, RetailerPriceHistoryDto>
{
    public Task<RetailerPriceHistoryDto> Handle(
        RetailerPriceHistoryQuery request,
        CancellationToken cancellationToken) =>
        service.GetHistoryAsync(
            request.ProductId,
            request.RetailerId,
            request.PageIndex,
            request.PageSize,
            cancellationToken);
}
