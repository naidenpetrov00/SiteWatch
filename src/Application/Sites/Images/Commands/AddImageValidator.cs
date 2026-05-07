using FluentValidation;

namespace Application.Sites.Images.Commands;

public class AddImageValidator : AbstractValidator<AddImageCommand>
{
    public AddImageValidator()
    {
        RuleFor(ai => ai.SiteId).NotEmpty();
        RuleFor(ai => ai.File).NotNull();
    }
}