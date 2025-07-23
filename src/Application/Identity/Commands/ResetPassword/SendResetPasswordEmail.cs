using Application.SeedWork.Enums;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Identity.Commands.ResetPassword;

public class SendResetPasswordEmailCommand : IRequest<IdentityResultModel>
{
    public required string Email { get; set; }
}

public class SendResetPasswordEmailHandler
    : IRequestHandler<SendResetPasswordEmailCommand, IdentityResultModel>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IIdentityService _identityService;

    public SendResetPasswordEmailHandler(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        IIdentityService identityService
    )
    {
        _userManager = userManager;
        _emailService = emailService;
        _identityService = identityService;
    }

    public async Task<IdentityResultModel> Handle(
        SendResetPasswordEmailCommand request,
        CancellationToken cancellationToken
    )
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return new IdentityResultOnly
            {
                Result = Result.Failure([IdentityResultErrors.UserNotFound]),
            };

        var token = _identityService.GenerateVerificationToken();

        await _userManager.SetAuthenticationTokenAsync(
            user,
            EmailProvider.Password.ToString(),
            EmailProvider.SMTP.ToString(),
            token
        );

        await _emailService.SendPasswordResetEmailAsync(user.Email!, token);

        return new IdentityResultOnly { Result = Result.Success() };
    }
}
