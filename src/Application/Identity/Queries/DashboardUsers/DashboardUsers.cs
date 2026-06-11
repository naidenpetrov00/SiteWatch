using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Application.SeedWork.Security;
using Application.SeedWork.Queries;
using Domain.Entities;
using MediatR;

namespace Application.Identity.Queries.DashboardUsers;

[Authorize(Roles = UserClaimTypes.Administrator)]
public class DashboardUsersQuery : TableQueryRequest, IRequest<PagedResult<DashboardUserDto>>
{
    public string? Id { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? IsEmailConfirmed { get; set; }
    public bool? IsPhoneNumberConfirmed { get; set; }
    public string? LastLoginAt { get; set; }

    public static readonly TableQueryDefinition<ApplicationUser, DashboardUsersQuery> Table =
        new(
            Filters:
            [
                TableFilterDescriptor<ApplicationUser, DashboardUsersQuery>.TextContains(
                    "id",
                    query => query.Id,
                    user => user.Id
                ),
                TableFilterDescriptor<ApplicationUser, DashboardUsersQuery>.TextContains(
                    "username",
                    query => query.Username,
                    user => user.UserName ?? string.Empty
                ),
                TableFilterDescriptor<ApplicationUser, DashboardUsersQuery>.TextContains(
                    "email",
                    query => query.Email,
                    user => user.Email ?? string.Empty
                ),
                TableFilterDescriptor<ApplicationUser, DashboardUsersQuery>.TextContains(
                    "phoneNumber",
                    query => query.PhoneNumber,
                    user => user.PhoneNumber ?? string.Empty
                ),
                TableFilterDescriptor<ApplicationUser, DashboardUsersQuery>.BooleanEquals(
                    "isEmailConfirmed",
                    query => query.IsEmailConfirmed,
                    user => user.EmailConfirmed
                ),
                TableFilterDescriptor<ApplicationUser, DashboardUsersQuery>.BooleanEquals(
                    "isPhoneNumberConfirmed",
                    query => query.IsPhoneNumberConfirmed,
                    user => user.PhoneNumberConfirmed
                ),
                TableFilterDescriptor<ApplicationUser, DashboardUsersQuery>.DateTimeOffsetSearch(
                    "lastLoginAt",
                    query => query.LastLoginAt,
                    user => user.LastLoginAt
                )
            ],
            Sorts: new Dictionary<string, TableSortDescriptor<ApplicationUser, DashboardUsersQuery>>(
                StringComparer.OrdinalIgnoreCase
            )
            {
                ["id"] = TableSortDescriptor<ApplicationUser, DashboardUsersQuery>.Create(
                    "id",
                    user => user.Id,
                    user => user.Id
                ),
                ["username"] = TableSortDescriptor<ApplicationUser, DashboardUsersQuery>.Create(
                    "username",
                    user => user.UserName ?? string.Empty,
                    user => user.Id
                ),
                ["email"] = TableSortDescriptor<ApplicationUser, DashboardUsersQuery>.Create(
                    "email",
                    user => user.Email ?? string.Empty,
                    user => user.Id
                ),
                ["phoneNumber"] = TableSortDescriptor<ApplicationUser, DashboardUsersQuery>.Create(
                    "phoneNumber",
                    user => user.PhoneNumber ?? string.Empty,
                    user => user.Id
                ),
                ["isEmailConfirmed"] = TableSortDescriptor<ApplicationUser, DashboardUsersQuery>.Create(
                    "isEmailConfirmed",
                    user => user.EmailConfirmed,
                    user => user.Id
                ),
                ["isPhoneNumberConfirmed"] = TableSortDescriptor<ApplicationUser, DashboardUsersQuery>.Create(
                    "isPhoneNumberConfirmed",
                    user => user.PhoneNumberConfirmed,
                    user => user.Id
                ),
                ["lastLoginAt"] = TableSortDescriptor<ApplicationUser, DashboardUsersQuery>.Create(
                    "lastLoginAt",
                    user => user.LastLoginAt ?? DateTimeOffset.MinValue,
                    user => user.Id
                )
            },
            DefaultSort: query =>
                query
                    .OrderBy(user => user.UserName ?? string.Empty)
                    .ThenBy(user => user.Email ?? string.Empty)
                    .ThenBy(user => user.Id)
        );
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
