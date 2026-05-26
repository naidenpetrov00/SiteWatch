using Application.SeedWork.Interfaces;
using FluentValidation;
using Domain.SeedWork.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Sites.Images.Commands;

public class AddImageValidator : AbstractValidator<AddImageCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public AddImageValidator(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;

        RuleFor(ai => ai.SiteId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MustAsync(SiteIdMustExist)
            .WithMessage("Site does not exist.");

        RuleFor(ai => ai.File).NotNull();
        RuleFor(ai => ai.Category)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(category => category.HasValue && Enum.IsDefined(typeof(ImageCategory), category.Value))
            .WithMessage("Category is not valid.")
            .MustAsync((request, category, cancellationToken) => ImageCategoryAllowedForSite(request, category, cancellationToken))
            .WithMessage("Category is not allowed for this site.");
    }

    private async Task<bool> SiteIdMustExist(Guid siteId, CancellationToken cancellationToken) =>
        await _dbContext.Sites.AsNoTracking().AnyAsync(site => site.Id == siteId, cancellationToken);

    private async Task<bool> ImageCategoryAllowedForSite(
        AddImageCommand request,
        ImageCategory? category,
        CancellationToken cancellationToken)
    {
        if (!category.HasValue)
        {
            return false;
        }

        var site = await _dbContext.Sites.AsNoTracking()
            .FirstOrDefaultAsync(site => site.Id == request.SiteId, cancellationToken);

        return site is null || site.MediaPolicy.AllowsImageCategory(category.Value);
    }
}
