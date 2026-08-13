using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using FluentValidation;
using MediatR;

namespace Application.Identity.Queries.DashboardUsers;

[Authorize(Roles = UserRoles.Administrator)]
public sealed record DashboardUserSearchQuery : IRequest<List<DashboardUserLookupDto>>
{
    public string? SearchTerm { get; init; }
}

public sealed class DashboardUserSearchQueryValidator : AbstractValidator<DashboardUserSearchQuery>
{
    public DashboardUserSearchQueryValidator()
    {
        RuleFor(query => query.SearchTerm).MaximumLength(200);
    }
}

public sealed class DashboardUserSearchQueryHandler(IIdentityService identityService)
    : IRequestHandler<DashboardUserSearchQuery, List<DashboardUserLookupDto>>
{
    public Task<List<DashboardUserLookupDto>> Handle(
        DashboardUserSearchQuery request,
        CancellationToken cancellationToken) =>
        identityService.SearchUsersAsync(request.SearchTerm, cancellationToken);
}
