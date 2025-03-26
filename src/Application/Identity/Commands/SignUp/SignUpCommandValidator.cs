using FluentValidation;

namespace Application.Identity.Commands.SignUp;

public class SignUpCommandValidator : AbstractValidator<SignUpCommand>
{
    public SignUpCommandValidator()
    {
        RuleFor(su => su.Email).NotEmpty().EmailAddress();
        RuleFor(su => su.FullName).NotEmpty().MaximumLength(50);
        RuleFor(su => su.Password)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(20)
            .Must(HaveOneUpercaseAndOneDigit);
    }

    private bool HaveOneUpercaseAndOneDigit(SignUpCommand model, string password)
    {
        bool hasUppercase = password.Any(char.IsUpper);
        bool hasDigit = password.Any(char.IsDigit);
        return hasUppercase && hasDigit;
    }
}
