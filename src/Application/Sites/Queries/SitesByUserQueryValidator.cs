using Application.SeedWork.Interfaces;
using FluentValidation;

namespace Application.Sites.Queries;

public class SitesByUserQueryValidator : AbstractValidator<SitesByUserQuery>
{
    private readonly IIdentityService _identityService;

    public SitesByUserQueryValidator(IIdentityService identityService)
    {
        _identityService = identityService;
        RuleFor(sq => sq.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.")
            .MustAsync(UserExists)
            .WithMessage("User not found.");
    }

    private async Task<bool> UserExists(Guid userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var userName = await _identityService.GetUserNameAsync(userId.ToString());
        return userName is not null;
    }
}
