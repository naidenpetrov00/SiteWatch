using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Application.SeedWork.Security;
using Application.SeedWork.Queries;
using MediatR;

namespace Application.Identity.Queries.DashboardUsers;

[Authorize(Roles = UserClaimTypes.Administrator)]
public sealed partial class DashboardUsersQuery
    : TableQueryRequest,
        IRequest<PagedResult<DashboardUserDto>>
{
    public string? Id { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? IsEmailConfirmed { get; set; }
    public bool? IsPhoneNumberConfirmed { get; set; }
    public string? LastLoginAt { get; set; }
}

public class DashboardUsersQueryHandler
    : IRequestHandler<DashboardUsersQuery, PagedResult<DashboardUserDto>>
{
    private readonly IIdentityService _identityService;

    public DashboardUsersQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<PagedResult<DashboardUserDto>> Handle(
        DashboardUsersQuery request,
        CancellationToken cancellationToken
    )
        => _identityService.GetUsersAsync(request, cancellationToken);
}
