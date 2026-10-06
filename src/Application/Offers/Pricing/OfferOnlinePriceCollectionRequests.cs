using Application.RetailerPriceCollections;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using FluentValidation;
using MediatR;

namespace Application.Offers.Pricing;

/// <summary>Loads server-derived retailer-company collection choices for an offer.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record OfferOnlinePriceCollectionOptionsQuery(Guid SiteId, Guid OfferId)
    : IRequest<OfferOnlinePriceCollectionOptionsDto>;

/// <summary>Starts one offer-scoped durable run per selected retailer company.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record StartOfferOnlinePriceCollectionCommand
    : IRequest<OfferOnlinePriceCollectionStartResponseDto>
{
    public Guid SiteId { get; set; }
    public Guid OfferId { get; set; }
    public IReadOnlyList<Guid> CompanyPersonIds { get; init; } = [];
}

/// <summary>Loads recent durable runs admitted from an offer.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record OfferOnlinePriceCollectionRunsQuery(
    Guid SiteId,
    Guid OfferId,
    int Limit = 25) : IRequest<IReadOnlyList<RetailerPriceCollectionRunSummaryDto>>;

public sealed class OfferOnlinePriceCollectionOptionsValidator
    : AbstractValidator<OfferOnlinePriceCollectionOptionsQuery>
{
    public OfferOnlinePriceCollectionOptionsValidator()
    {
        RuleFor(query => query.SiteId).NotEmpty();
        RuleFor(query => query.OfferId).NotEmpty();
    }
}

public sealed class StartOfferOnlinePriceCollectionValidator
    : AbstractValidator<StartOfferOnlinePriceCollectionCommand>
{
    public StartOfferOnlinePriceCollectionValidator()
    {
        RuleFor(command => command.SiteId).NotEmpty();
        RuleFor(command => command.OfferId).NotEmpty();
        RuleFor(command => command.CompanyPersonIds).NotEmpty();
        RuleForEach(command => command.CompanyPersonIds).NotEmpty();
        RuleFor(command => command.CompanyPersonIds)
            .Must(ids => ids.Count == ids.Distinct().Count())
            .WithMessage("CompanyPersonIds must not contain duplicates.");
    }
}

public sealed class OfferOnlinePriceCollectionRunsValidator
    : AbstractValidator<OfferOnlinePriceCollectionRunsQuery>
{
    public OfferOnlinePriceCollectionRunsValidator()
    {
        RuleFor(query => query.SiteId).NotEmpty();
        RuleFor(query => query.OfferId).NotEmpty();
        RuleFor(query => query.Limit).InclusiveBetween(1, 100);
    }
}

public sealed class OfferOnlinePriceCollectionOptionsHandler(
    IOfferOnlinePriceCollectionService service)
    : IRequestHandler<OfferOnlinePriceCollectionOptionsQuery, OfferOnlinePriceCollectionOptionsDto>
{
    public Task<OfferOnlinePriceCollectionOptionsDto> Handle(
        OfferOnlinePriceCollectionOptionsQuery request,
        CancellationToken cancellationToken) =>
        service.GetOptionsAsync(request.SiteId, request.OfferId, cancellationToken);
}

public sealed class StartOfferOnlinePriceCollectionHandler(
    IOfferOnlinePriceCollectionService service)
    : IRequestHandler<StartOfferOnlinePriceCollectionCommand, OfferOnlinePriceCollectionStartResponseDto>
{
    public Task<OfferOnlinePriceCollectionStartResponseDto> Handle(
        StartOfferOnlinePriceCollectionCommand request,
        CancellationToken cancellationToken) =>
        service.StartAsync(request, cancellationToken);
}

public sealed class OfferOnlinePriceCollectionRunsHandler(
    IOfferOnlinePriceCollectionService service)
    : IRequestHandler<
        OfferOnlinePriceCollectionRunsQuery,
        IReadOnlyList<RetailerPriceCollectionRunSummaryDto>>
{
    public Task<IReadOnlyList<RetailerPriceCollectionRunSummaryDto>> Handle(
        OfferOnlinePriceCollectionRunsQuery request,
        CancellationToken cancellationToken) =>
        service.GetRecentAsync(
            request.SiteId,
            request.OfferId,
            request.Limit,
            cancellationToken);
}
