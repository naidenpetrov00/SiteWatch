using FluentValidation;

namespace Application.Identity.Queries.Users;

public class UsersQueryValidator : AbstractValidator<UsersQuery>
{
    public UsersQueryValidator()
    {
        RuleFor(uq => uq.Email).NotNull().NotEmpty().EmailAddress();
    }
}
