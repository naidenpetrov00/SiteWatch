using Application.SeedWork.Interfaces;
using FluentValidation;
using Domain.ValueObjects;
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
            .Must(category => SiteMediaPolicy.NormalizeCategory(category) is not null)
            .WithMessage("Category is not valid.")
            .Must(category => SiteMediaPolicy.NormalizeCategory(category)!.Length <= SiteMediaPolicy.MaxCategoryLength)
            .WithMessage($"Category cannot exceed {SiteMediaPolicy.MaxCategoryLength} characters.")
            .MustAsync((request, category, cancellationToken) => MediaCategoryAllowedForSite(request, category, cancellationToken))
            .WithMessage("Category is not allowed for this site.");
    }

    private async Task<bool> SiteIdMustExist(Guid siteId, CancellationToken cancellationToken) =>
        await _dbContext.Sites.AsNoTracking().AnyAsync(site => site.Id == siteId, cancellationToken);

    private async Task<bool> MediaCategoryAllowedForSite(
        AddImageCommand request,
        string? category,
        CancellationToken cancellationToken)
    {
        if (SiteMediaPolicy.NormalizeCategory(category) is null)
        {
            return false;
        }

        var site = await _dbContext.Sites.AsNoTracking()
            .FirstOrDefaultAsync(site => site.Id == request.SiteId, cancellationToken);

        return site is null || site.MediaPolicy.AllowsCategory(category);
    }
}
