using FluentValidation;

namespace Application.Identity.Commands.ResetPassword;

public class SendResetPasswordEmailValidator : AbstractValidator<SendResetPasswordEmailCommand>
{
    public SendResetPasswordEmailValidator()
    {
        RuleFor(rp => rp.Email).NotEmpty().EmailAddress();
    }
}
