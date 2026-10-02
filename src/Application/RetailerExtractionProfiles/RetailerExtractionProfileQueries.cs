using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using MediatR;

namespace Application.RetailerExtractionProfiles;

/// <summary>Loads all extraction-profile versions for a company Person.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record RetailerExtractionProfileVersionsQuery(Guid CompanyPersonId)
    : IRequest<IReadOnlyList<RetailerExtractionProfileSummaryDto>>;

/// <summary>Loads the current draft and active extraction profile for a company Person.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record RetailerExtractionCurrentProfilesQuery(Guid CompanyPersonId)
    : IRequest<RetailerExtractionCurrentProfilesDto>;

/// <summary>Loads read-only active extraction status for a company Person.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record RetailerExtractionOverviewQuery(Guid CompanyPersonId)
    : IRequest<RetailerExtractionOverviewDto>;

/// <summary>Loads a complete extraction-profile version owned by a company Person.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record RetailerExtractionProfileByIdQuery(Guid CompanyPersonId, Guid ProfileId)
    : IRequest<RetailerExtractionProfileDetailsDto>;

public sealed class RetailerExtractionProfileVersionsHandler(
    IRetailerExtractionProfileService service)
    : IRequestHandler<
        RetailerExtractionProfileVersionsQuery,
        IReadOnlyList<RetailerExtractionProfileSummaryDto>>
{
    public Task<IReadOnlyList<RetailerExtractionProfileSummaryDto>> Handle(
        RetailerExtractionProfileVersionsQuery request,
        CancellationToken cancellationToken) =>
        service.GetVersionsAsync(request.CompanyPersonId, cancellationToken);
}

public sealed class RetailerExtractionCurrentProfilesHandler(
    IRetailerExtractionProfileService service)
    : IRequestHandler<RetailerExtractionCurrentProfilesQuery, RetailerExtractionCurrentProfilesDto>
{
    public Task<RetailerExtractionCurrentProfilesDto> Handle(
        RetailerExtractionCurrentProfilesQuery request,
        CancellationToken cancellationToken) =>
        service.GetCurrentAsync(request.CompanyPersonId, cancellationToken);
}

public sealed class RetailerExtractionOverviewHandler(
    IRetailerExtractionProfileService service)
    : IRequestHandler<RetailerExtractionOverviewQuery, RetailerExtractionOverviewDto>
{
    public Task<RetailerExtractionOverviewDto> Handle(
        RetailerExtractionOverviewQuery request,
        CancellationToken cancellationToken) =>
        service.GetOverviewAsync(request.CompanyPersonId, cancellationToken);
}

public sealed class RetailerExtractionProfileByIdHandler(
    IRetailerExtractionProfileService service)
    : IRequestHandler<RetailerExtractionProfileByIdQuery, RetailerExtractionProfileDetailsDto>
{
    public Task<RetailerExtractionProfileDetailsDto> Handle(
        RetailerExtractionProfileByIdQuery request,
        CancellationToken cancellationToken) =>
        service.GetByIdAsync(request.CompanyPersonId, request.ProfileId, cancellationToken);
}
