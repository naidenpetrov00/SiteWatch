using Application.SeedWork.Enums;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Identity.Commands.Email;

public class SendEmailVerificationCommand : IRequest<IdentityResultModel>
{
    public required string Email { get; set; }
}

public class SendEmailVerificationHandler
    : IRequestHandler<SendEmailVerificationCommand, IdentityResultModel>
{
    private readonly IIdentityService _identityService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;

    public SendEmailVerificationHandler(
        IIdentityService identityService,
        UserManager<ApplicationUser> userManager,
        IEmailService emailService
    )
    {
        _identityService = identityService;
        _userManager = userManager;
        _emailService = emailService;
    }

    public async Task<IdentityResultModel> Handle(
        SendEmailVerificationCommand request,
        CancellationToken cancellationToken
    )
    {
        var user = await _identityService.FindUserByEmailAsync(request.Email);
        if (user == null && user?.Email == null)
            return new IdentityResultOnly
            {
                Result = Result.Failure([IdentityResultErrors.UserNotFound]),
            };

        if (user.EmailConfirmed)
            return new IdentityResultOnly
            {
                Result = Result.Failure([IdentityResultErrors.EmailAlreadyRegistered]),
            };

        var token = _identityService.GenerateVerificationToken();

        await _userManager.SetAuthenticationTokenAsync(
            user,
            EmailProvider.Email.ToString(),
            EmailProvider.SMTP.ToString(),
            token
        );

        await _emailService.SendVerifyEmailAsync(user.Email!, token);

        return new IdentityResultOnly { Result = Result.Success() };
    }
}
