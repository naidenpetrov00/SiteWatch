using Application.Identity.Commands;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using AutoMapper;
using MediatR;

namespace Application.Identity.Queries.Users;

public class UsersQuery : IRequest<IdentityResultModel>
{
    public required string Email { get; set; }
}

public class UsersQueryHandler : IRequestHandler<UsersQuery, IdentityResultModel>
{
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;

    public UsersQueryHandler(IIdentityService identityService, IMapper mapper)
    {
        _identityService = identityService;
        _mapper = mapper;
    }

    public async Task<IdentityResultModel> Handle(
        UsersQuery request,
        CancellationToken cancellationToken
    )
    {
        var user = await _identityService.FindUserByEmailAsync(request.Email);
        if (user != null)
        {
            var userDto = _mapper.Map<UserInfoDto>(user);
            return new IdentityResultWithUser { Result = Result.Success(), User = userDto };
        }

        return new IdentityResultOnly
        {
            Result = Result.Failure([IdentityResultErrors.UserNotFound]),
        };
    }
}
