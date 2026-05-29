using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using MediatR;

namespace Application.Identity.Commands.DashboardSignIn;

public class DashboardSignInCommand : IRequest<IdentityResultModel>
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class DashboardSignInHandler : IRequestHandler<DashboardSignInCommand, IdentityResultModel>
{
    private readonly IIdentityService _identityService;

    public DashboardSignInHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<IdentityResultModel> Handle(
        DashboardSignInCommand request,
        CancellationToken cancellationToken
    )
    {
        var user = await _identityService.FindUserByEmailAsync(request.Email);
        if (user == null)
        {
            return new IdentityResultOnly
            {
                Result = new Result(false, [IdentityResultErrors.InvalidCredentials]),
            };
        }

        return await _identityService.CheckAdministratorPasswordAsync(user, request.Password);
    }
}
