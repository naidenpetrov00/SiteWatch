using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using MediatR;

namespace Application.RetailerExtractionProfiles;

/// <summary>Loads all extraction-profile versions for a retailer.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record RetailerExtractionProfileVersionsQuery(Guid RetailerId)
    : IRequest<IReadOnlyList<RetailerExtractionProfileSummaryDto>>;

/// <summary>Loads the current draft and active extraction profile for a retailer.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record RetailerExtractionCurrentProfilesQuery(Guid RetailerId)
    : IRequest<RetailerExtractionCurrentProfilesDto>;

/// <summary>Loads a complete extraction-profile version owned by a retailer.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record RetailerExtractionProfileByIdQuery(Guid RetailerId, Guid ProfileId)
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
        service.GetVersionsAsync(request.RetailerId, cancellationToken);
}

public sealed class RetailerExtractionCurrentProfilesHandler(
    IRetailerExtractionProfileService service)
    : IRequestHandler<RetailerExtractionCurrentProfilesQuery, RetailerExtractionCurrentProfilesDto>
{
    public Task<RetailerExtractionCurrentProfilesDto> Handle(
        RetailerExtractionCurrentProfilesQuery request,
        CancellationToken cancellationToken) =>
        service.GetCurrentAsync(request.RetailerId, cancellationToken);
}

public sealed class RetailerExtractionProfileByIdHandler(
    IRetailerExtractionProfileService service)
    : IRequestHandler<RetailerExtractionProfileByIdQuery, RetailerExtractionProfileDetailsDto>
{
    public Task<RetailerExtractionProfileDetailsDto> Handle(
        RetailerExtractionProfileByIdQuery request,
        CancellationToken cancellationToken) =>
        service.GetByIdAsync(request.RetailerId, request.ProfileId, cancellationToken);
}
