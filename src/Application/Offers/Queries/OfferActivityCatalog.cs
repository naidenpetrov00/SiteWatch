using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using FluentValidation;
using MediatR;

namespace Application.Offers.Queries;

/// <summary>Browses or searches the Activity Catalog in the context of an Offer.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record OfferActivityCatalogQuery
    : IRequest<IReadOnlyList<OfferActivityCatalogNodeDto>>
{
    public Guid SiteId { get; set; }
    public Guid OfferId { get; set; }
    public string? SearchTerm { get; init; }
}

/// <summary>Represents one catalog node and whether it is already selected.</summary>
public sealed record OfferActivityCatalogNodeDto(
    Guid Id,
    string Kind,
    Guid? ParentFolderId,
    string Name,
    int SortOrder,
    int? NumberId,
    string? Status,
    bool IsSelected);

public sealed class OfferActivityCatalogHandler(IOfferService offerService)
    : IRequestHandler<OfferActivityCatalogQuery, IReadOnlyList<OfferActivityCatalogNodeDto>>
{
    public Task<IReadOnlyList<OfferActivityCatalogNodeDto>> Handle(
        OfferActivityCatalogQuery request,
        CancellationToken cancellationToken) =>
        offerService.GetActivityCatalogAsync(request, cancellationToken);
}

/// <summary>Loads an active Activity Catalog item before it is added to an Offer.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record OfferActivityCandidateQuery(
    Guid SiteId,
    Guid OfferId,
    Guid ActivityId) : IRequest<OfferActivityCandidateDto>;

/// <summary>Represents a section that requires an initial requested measurement.</summary>
public sealed record OfferActivityCandidateSectionDto(
    Guid Id,
    string? Name,
    decimal BasisQuantity,
    string MeasurementUnit,
    int SortOrder);

/// <summary>Represents an active activity that can be selected for an Offer.</summary>
public sealed record OfferActivityCandidateDto(
    Guid Id,
    int NumberId,
    string Name,
    string? Description,
    IReadOnlyList<OfferActivityCandidateSectionDto> Sections);

public sealed class OfferActivityCandidateHandler(IOfferService offerService)
    : IRequestHandler<OfferActivityCandidateQuery, OfferActivityCandidateDto>
{
    public Task<OfferActivityCandidateDto> Handle(
        OfferActivityCandidateQuery request,
        CancellationToken cancellationToken) =>
        offerService.GetActivityCandidateAsync(
            request.SiteId,
            request.OfferId,
            request.ActivityId,
            cancellationToken);
}

public sealed class OfferActivityCatalogValidator
    : AbstractValidator<OfferActivityCatalogQuery>
{
    public OfferActivityCatalogValidator()
    {
        RuleFor(query => query.SiteId).NotEmpty();
        RuleFor(query => query.OfferId).NotEmpty();
        RuleFor(query => query.SearchTerm).MaximumLength(200);
    }
}

public sealed class OfferActivityCandidateValidator
    : AbstractValidator<OfferActivityCandidateQuery>
{
    public OfferActivityCandidateValidator()
    {
        RuleFor(query => query.SiteId).NotEmpty();
        RuleFor(query => query.OfferId).NotEmpty();
        RuleFor(query => query.ActivityId).NotEmpty();
    }
}
