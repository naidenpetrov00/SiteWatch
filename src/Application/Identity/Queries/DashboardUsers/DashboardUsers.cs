using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using AutoMapper;
using MediatR;

namespace Application.Identity.Queries.DashboardUsers;

[Authorize(Roles = UserClaimTypes.Administrator)]
public class DashboardUsersQuery : IRequest<List<DashboardUserDto>>
{
}

public class DashboardUsersQueryHandler : IRequestHandler<DashboardUsersQuery, List<DashboardUserDto>>
{
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;

    public DashboardUsersQueryHandler(IIdentityService identityService, IMapper mapper)
    {
        _identityService = identityService;
        _mapper = mapper;
    }

    public async Task<List<DashboardUserDto>> Handle(
        DashboardUsersQuery request,
        CancellationToken cancellationToken
    )
    {
        var users = await _identityService.GetUsersAsync();

        return _mapper.Map<List<DashboardUserDto>>(
            users.OrderBy(user => user.Username).ThenBy(user => user.Email).ToList()
        );
    }
}
