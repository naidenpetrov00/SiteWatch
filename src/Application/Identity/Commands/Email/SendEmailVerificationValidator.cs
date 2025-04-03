using FluentValidation;

namespace Application.Identity.Commands.Email;

public class SendEmailVerificationValidator : AbstractValidator<SendEmailVerificationCommand>
{
    public SendEmailVerificationValidator()
    {
        RuleFor(se => se.Email).NotEmpty().EmailAddress();
    }
}
