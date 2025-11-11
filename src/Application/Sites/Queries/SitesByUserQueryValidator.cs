using FluentValidation;

namespace Application.Sites.Queries;

public class SitesByUserQueryValidator : AbstractValidator<SitesByUserQuery>
{
    public SitesByUserQueryValidator()
    {
        RuleFor(sq => sq.UserId).NotEmpty().WithMessage("UserId is required.");
    }
}