using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using FluentValidation;
using MediatR;

namespace Application.RetailerPriceCollections;

/// <summary>Starts durable collection for all listings owned by a company Person.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record StartRetailerPriceCollectionRunCommand(Guid CompanyPersonId)
    : IRequest<RetailerPriceCollectionRunSummaryDto>;

/// <summary>Loads the most recent price-collection runs for a company Person.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record RecentRetailerPriceCollectionRunsQuery(
    Guid CompanyPersonId,
    int Limit = 10) : IRequest<IReadOnlyList<RetailerPriceCollectionRunSummaryDto>>;

/// <summary>Loads one price-collection run with a page of item results.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record RetailerPriceCollectionRunByIdQuery(
    Guid CompanyPersonId,
    Guid RunId,
    int PageIndex = 0,
    int PageSize = 25) : IRequest<RetailerPriceCollectionRunDetailsDto>;

public sealed class StartRetailerPriceCollectionRunValidator
    : AbstractValidator<StartRetailerPriceCollectionRunCommand>
{
    public StartRetailerPriceCollectionRunValidator() =>
        RuleFor(command => command.CompanyPersonId).NotEmpty();
}

public sealed class RecentRetailerPriceCollectionRunsValidator
    : AbstractValidator<RecentRetailerPriceCollectionRunsQuery>
{
    public RecentRetailerPriceCollectionRunsValidator()
    {
        RuleFor(query => query.CompanyPersonId).NotEmpty();
        RuleFor(query => query.Limit).InclusiveBetween(1, 50);
    }
}

public sealed class RetailerPriceCollectionRunByIdValidator
    : AbstractValidator<RetailerPriceCollectionRunByIdQuery>
{
    public RetailerPriceCollectionRunByIdValidator()
    {
        RuleFor(query => query.CompanyPersonId).NotEmpty();
        RuleFor(query => query.RunId).NotEmpty();
        RuleFor(query => query.PageIndex).GreaterThanOrEqualTo(0);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class StartRetailerPriceCollectionRunHandler(
    IRetailerPriceCollectionService service)
    : IRequestHandler<StartRetailerPriceCollectionRunCommand, RetailerPriceCollectionRunSummaryDto>
{
    public Task<RetailerPriceCollectionRunSummaryDto> Handle(
        StartRetailerPriceCollectionRunCommand request,
        CancellationToken cancellationToken) =>
        service.StartAsync(request.CompanyPersonId, cancellationToken);
}

public sealed class RecentRetailerPriceCollectionRunsHandler(
    IRetailerPriceCollectionService service)
    : IRequestHandler<
        RecentRetailerPriceCollectionRunsQuery,
        IReadOnlyList<RetailerPriceCollectionRunSummaryDto>>
{
    public Task<IReadOnlyList<RetailerPriceCollectionRunSummaryDto>> Handle(
        RecentRetailerPriceCollectionRunsQuery request,
        CancellationToken cancellationToken) =>
        service.GetRecentAsync(request.CompanyPersonId, request.Limit, cancellationToken);
}

public sealed class RetailerPriceCollectionRunByIdHandler(
    IRetailerPriceCollectionService service)
    : IRequestHandler<RetailerPriceCollectionRunByIdQuery, RetailerPriceCollectionRunDetailsDto>
{
    public Task<RetailerPriceCollectionRunDetailsDto> Handle(
        RetailerPriceCollectionRunByIdQuery request,
        CancellationToken cancellationToken) =>
        service.GetByIdAsync(
            request.CompanyPersonId,
            request.RunId,
            request.PageIndex,
            request.PageSize,
            cancellationToken);
}
