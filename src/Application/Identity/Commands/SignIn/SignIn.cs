using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using MediatR;

namespace Application.Identity.Commands.SignIn;

public class SignInCommand : IRequest<IdentityResultModel>
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class SignInHandler : IRequestHandler<SignInCommand, IdentityResultModel>
{
    private readonly IIdentityService _identityService;

    public SignInHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<IdentityResultModel> Handle(
        SignInCommand request,
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

        return await _identityService.CheckPasswordAsync(user, request.Password);
    }
}
