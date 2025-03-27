using Application.SeedWork.Interfaces;
using MediatR;

namespace Api.Endpoints;

public class SignInCommand : IRequest<string> { }

public class SignInHandler : IRequestHandler<SignInCommand, string>
{
    private readonly IIdentityService _identityService;

    public SignInHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<string> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        return _identityService.AuthorizeAsync();
    }
}
