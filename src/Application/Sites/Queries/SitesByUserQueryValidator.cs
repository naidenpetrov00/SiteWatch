using Application.Sites.Queries;
using FluentValidation;

public class SitesByUserQueryValidator : AbstractValidator<SitesByUserQuery>
{
    public SitesByUserQueryValidator()
    {
        RuleFor(sq => sq.UserId).NotEmpty().WithMessage("UserId is required.");
    }
}
