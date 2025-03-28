using FluentValidation;

namespace Application.Identity.Commands.SignIn;

public class SignInCommandValidator : AbstractValidator<SignInCommand>
{
    public SignInCommandValidator()
    {
        RuleFor(si => si.Email).NotEmpty().NotNull().EmailAddress();
    }
}
