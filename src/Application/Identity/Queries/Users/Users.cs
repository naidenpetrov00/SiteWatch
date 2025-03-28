using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using MediatR;

namespace Application.Identity.Queries.Users;

public class UsersQuery : IRequest<IdentityResultModel>
{
    public required string Email { get; set; }
}

public class UsersQueryHandler : IRequestHandler<UsersQuery, IdentityResultModel>
{
    private readonly IIdentityService _identityService;

    public UsersQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<IdentityResultModel> Handle(
        UsersQuery request,
        CancellationToken cancellationToken
    )
    {
        var user = await _identityService.FindUserByEmailAsync(request.Email);
        if (user != null)
        {
            return new IdentityResultWithUser { Result = Result.Success(), User = user };
        }

        return new IdentityResultOnly
        {
            Result = Result.Failure([IdentityResultErrors.UserNotFound]),
        };
    }
}
