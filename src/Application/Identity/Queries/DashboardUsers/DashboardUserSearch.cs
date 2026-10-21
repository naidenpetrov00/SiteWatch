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
    public string[]? Roles { get; init; }
}

public sealed class DashboardUserSearchQueryValidator : AbstractValidator<DashboardUserSearchQuery>
{
    public DashboardUserSearchQueryValidator()
    {
        RuleFor(query => query.SearchTerm).MaximumLength(200);
        RuleFor(query => query.Role)
            .Must(role => string.IsNullOrWhiteSpace(role) || UserRoles.IsSupported(role.Trim()))
            .WithMessage("Role must be a supported user role.");
        RuleFor(query => query.Roles)
            .Must(roles => roles is null || roles.Length is > 0 and <= 2)
            .WithMessage("Roles must contain one or two eligible responsibility roles.");
        RuleForEach(query => query.Roles)
            .Must(role => !string.IsNullOrWhiteSpace(role)
                && (role.Trim() == UserRoles.Administrator || role.Trim() == UserRoles.Worker))
            .WithMessage("Roles must contain only Administrator or Worker.");
        RuleFor(query => query)
            .Must(query => string.IsNullOrWhiteSpace(query.Role)
                || query.Roles is not { Length: > 0 })
            .WithMessage("Specify either Role or Roles, not both.");
    }
}

public sealed class DashboardUserSearchQueryHandler(IIdentityService identityService)
    : IRequestHandler<DashboardUserSearchQuery, List<DashboardUserLookupDto>>
{
    public Task<List<DashboardUserLookupDto>> Handle(
        DashboardUserSearchQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Roles is { Length: > 0 })
        {
            return identityService.SearchUsersAsync(
                request.SearchTerm,
                request.Roles.Select(role => role.Trim()).Distinct(StringComparer.Ordinal).ToArray(),
                cancellationToken);
        }

        return string.IsNullOrWhiteSpace(request.Role)
            ? identityService.SearchUsersAsync(request.SearchTerm, cancellationToken)
            : identityService.SearchUsersAsync(request.SearchTerm, request.Role, cancellationToken);
    }
}
