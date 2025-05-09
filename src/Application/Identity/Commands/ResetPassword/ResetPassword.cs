using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using MediatR;

namespace Application.Identity.Commands.ResetPassword;

public class ResetPasswordCommand : IRequest<IdentityResultModel>
{
    public required string Email { get; set; }
    public required string Token { get; set; }
    public required string NewPassword { get; set; }
}

public class ResetPasswordCommandHandler
    : IRequestHandler<ResetPasswordCommand, IdentityResultModel>
{
    private readonly IIdentityService _identityService;

    public ResetPasswordCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<IdentityResultModel> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken
    )
    {
        var user = await _identityService.FindUserByEmailAsync(request.Email);
        if (user == null)
            return new IdentityResultOnly
            {
                Result = Result.Failure([IdentityResultErrors.UserNotFound]),
            };

        return await _identityService.ResetPasswordAsync(user, request.Token, request.NewPassword);
    }
}
