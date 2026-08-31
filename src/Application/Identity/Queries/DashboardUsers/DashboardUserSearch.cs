using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using FluentValidation;
using MediatR;

namespace Application.Identity.Queries.DashboardUsers;

[Authorize(Roles = UserRoles.Administrator)]
public sealed record DashboardUserSearchQuery : IRequest<List<DashboardUserLookupDto>>
{
    public string? SearchTerm { get; init; }
    public string? Role { get; init; }
}

public sealed class DashboardUserSearchQueryValidator : AbstractValidator<DashboardUserSearchQuery>
{
    public DashboardUserSearchQueryValidator()
    {
        RuleFor(query => query.SearchTerm).MaximumLength(200);
        RuleFor(query => query.Role)
            .Must(role => string.IsNullOrWhiteSpace(role) || UserRoles.IsSupported(role.Trim()))
            .WithMessage("Role must be a supported user role.");
    }
}

public sealed class DashboardUserSearchQueryHandler(IIdentityService identityService)
    : IRequestHandler<DashboardUserSearchQuery, List<DashboardUserLookupDto>>
{
    public Task<List<DashboardUserLookupDto>> Handle(
        DashboardUserSearchQuery request,
        CancellationToken cancellationToken)
    {
        return string.IsNullOrWhiteSpace(request.Role)
            ? identityService.SearchUsersAsync(request.SearchTerm, cancellationToken)
            : identityService.SearchUsersAsync(request.SearchTerm, request.Role, cancellationToken);
    }
}
