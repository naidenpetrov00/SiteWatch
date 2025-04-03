using FluentValidation;

namespace Application.Identity.Commands.Email;

public class VerifyEmailValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailValidator()
    {
        RuleFor(ve => ve.Email).NotEmpty().EmailAddress();
        RuleFor(ve => ve.Token).NotEmpty();
    }
}
