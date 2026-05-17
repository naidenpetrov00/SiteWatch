using FluentValidation;

namespace Application.Sites.Videos.Commands;

public class AddVideoValidator : AbstractValidator<AddVideoCommand>
{
    public AddVideoValidator()
    {
        RuleFor(av => av.SiteId).NotEmpty();
        RuleFor(av => av.File).NotNull();
    }
}
