using FluentValidation;

namespace Application.Identity.Commands.ResetPassword;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordValidator()
    {
        RuleFor(rp => rp.Email).NotEmpty().EmailAddress();
        RuleFor(rp => rp.Token).NotEmpty().Length(6);
        RuleFor(rp => rp.NewPassword)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(20)
            .Must(HaveOneUpercaseAndOneDigit);
    }

    private bool HaveOneUpercaseAndOneDigit(string password)
    {
        bool hasUppercase = password.Any(char.IsUpper);
        bool hasDigit = password.Any(char.IsDigit);
        return hasUppercase && hasDigit;
    }
}
