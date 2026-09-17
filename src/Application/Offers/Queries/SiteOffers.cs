using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Application.SeedWork.Queries;
using Application.SeedWork.Security;
using MediatR;

namespace Application.Offers.Queries;

/// <summary>Defines filtering, sorting, and paging for a site's offers.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed partial class SiteOffersQuery
    : TableQueryRequest, IRequest<PagedResult<OfferSummaryDto>>
{
    public Guid SiteId { get; set; }
    public string? NumberId { get; set; }
    public string? Title { get; set; }
    public string? Status { get; set; }
}

public sealed class SiteOffersHandler(IOfferService offerService)
    : IRequestHandler<SiteOffersQuery, PagedResult<OfferSummaryDto>>
{
    public Task<PagedResult<OfferSummaryDto>> Handle(
        SiteOffersQuery request,
        CancellationToken cancellationToken) =>
        offerService.GetBySiteAsync(request, cancellationToken);
}
