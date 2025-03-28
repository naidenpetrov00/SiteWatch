using Application.SeedWork.Interfaces;
using MediatR;

namespace Application.Identity.Commands.SignUp;

public class SignUpCommand : IRequest<IdentityResultModel>
{
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class SignUpHandler : IRequestHandler<SignUpCommand, IdentityResultModel>
{
    private readonly IIdentityService identityService;

    public SignUpHandler(IIdentityService identityService)
    {
        this.identityService = identityService;
    }

    public Task<IdentityResultModel> Handle(
        SignUpCommand request,
        CancellationToken cancellationToken
    )
    {
        return identityService.CreateUserAsync(request.UserName, request.Email, request.Password);
    }
}
