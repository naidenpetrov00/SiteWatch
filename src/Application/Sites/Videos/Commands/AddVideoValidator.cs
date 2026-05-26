using Application.SeedWork.Interfaces;
using FluentValidation;
using Domain.SeedWork.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Sites.Videos.Commands;

public class AddVideoValidator : AbstractValidator<AddVideoCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public AddVideoValidator(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;

        RuleFor(av => av.SiteId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MustAsync(SiteIdMustExist)
            .WithMessage("Site does not exist.");

        RuleFor(av => av.File).NotNull();
        RuleFor(av => av.Category)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(category => category.HasValue && Enum.IsDefined(typeof(VideoCategory), category.Value))
            .WithMessage("Category is not valid.")
            .MustAsync((request, category, cancellationToken) => VideoCategoryAllowedForSite(request, category, cancellationToken))
            .WithMessage("Category is not allowed for this site.");
    }

    private async Task<bool> SiteIdMustExist(Guid siteId, CancellationToken cancellationToken) =>
        await _dbContext.Sites.AsNoTracking().AnyAsync(site => site.Id == siteId, cancellationToken);

    private async Task<bool> VideoCategoryAllowedForSite(
        AddVideoCommand request,
        VideoCategory? category,
        CancellationToken cancellationToken)
    {
        if (!category.HasValue)
        {
            return false;
        }

        var site = await _dbContext.Sites.AsNoTracking()
            .FirstOrDefaultAsync(site => site.Id == request.SiteId, cancellationToken);

        return site is null || site.MediaPolicy.AllowsVideoCategory(category.Value);
    }
}
