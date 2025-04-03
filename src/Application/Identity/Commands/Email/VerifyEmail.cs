using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using MediatR;

namespace Application.Identity.Commands.Email;

public class VerifyEmailCommand : IRequest<IdentityResultModel>
{
    public required string Email { get; set; }
    public required string Token { get; set; }
}

public class VerifyEmailCommandHandler(IIdentityService identityService)
    : IRequestHandler<VerifyEmailCommand, IdentityResultModel>
{
    private readonly IIdentityService _identityService = identityService;

    public async Task<IdentityResultModel> Handle(
        VerifyEmailCommand request,
        CancellationToken cancellationToken
    )
    {
        var user = await _identityService.FindUserByEmailAsync(request.Email);
        if (user == null)
            return new IdentityResultOnly
            {
                Result = Result.Failure([IdentityResultErrors.UserNotFound]),
            };

        return await _identityService.ConfirmEmailAsync(user, request.Token);
    }
}
