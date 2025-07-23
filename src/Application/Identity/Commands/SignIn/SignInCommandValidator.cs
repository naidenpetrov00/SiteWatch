using Application.SeedWork.Interfaces;
using FluentValidation;

namespace Application.Identity.Commands.SignIn;

public class SignInCommandValidator : AbstractValidator<SignInCommand>
{
    private readonly IIdentityService _identityService;

    public SignInCommandValidator(IIdentityService identityService)
    {
        _identityService = identityService;
        RuleFor(si => si.Email)
            .NotEmpty()
            .NotNull()
            .EmailAddress()
            .MustAsync(IsVerifiedEmail)
            .WithMessage("Email must be verified before signing in.");
    }

    private async Task<bool> IsVerifiedEmail(string email, CancellationToken cancellationToken) =>
        await _identityService.IsVerifiedEmailAsync(email);
}
