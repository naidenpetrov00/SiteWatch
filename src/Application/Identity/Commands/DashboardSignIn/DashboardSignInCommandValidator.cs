using Application.SeedWork.Interfaces;
using FluentValidation;

namespace Application.Identity.Commands.DashboardSignIn;

public class DashboardSignInCommandValidator : AbstractValidator<DashboardSignInCommand>
{
    private readonly IIdentityService _identityService;

    public DashboardSignInCommandValidator(IIdentityService identityService)
    {
        _identityService = identityService;
        RuleFor(signIn => signIn.Email)
            .NotEmpty()
            .NotNull()
            .EmailAddress()
            .MustAsync(IsVerifiedEmail)
            .WithMessage("Email must be verified before signing in.");
    }

    private async Task<bool> IsVerifiedEmail(string email, CancellationToken cancellationToken) =>
        await _identityService.IsVerifiedEmailAsync(email);
}
