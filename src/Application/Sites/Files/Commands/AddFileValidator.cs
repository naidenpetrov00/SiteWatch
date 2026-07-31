using Application.SeedWork.Interfaces;
using Domain.SeedWork.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Sites.Files.Commands;

public class AddFileValidator : AbstractValidator<AddFileCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public AddFileValidator(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;

        RuleFor(af => af.SiteId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MustAsync(SiteIdMustExist)
            .WithMessage("Site does not exist.");

        RuleFor(af => af.File)
            .NotNull()
            .DependentRules(() =>
            {
                RuleFor(af => af.File.FileName)
                    .NotEmpty();

                RuleFor(af => af.File.ContentType)
                    .NotEmpty();
            });

        RuleFor(af => af.Category)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(category => category.HasValue && Enum.IsDefined(typeof(FileCategory), category.Value))
            .WithMessage("Category is not valid.")
            .MustAsync((request, category, cancellationToken) =>
                FileCategoryAllowedForSite(request, category, cancellationToken))
            .WithMessage("Category is not allowed for this site.");

        RuleFor(af => af.DocumentType)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(documentType =>
                documentType.HasValue
                && Enum.IsDefined(typeof(FileDocumentType), documentType.Value))
            .WithMessage("Document type is not valid.");
    }

    private async Task<bool> SiteIdMustExist(Guid siteId, CancellationToken cancellationToken) =>
        await _dbContext.Sites.AsNoTracking().AnyAsync(site => site.Id == siteId, cancellationToken);

    private async Task<bool> FileCategoryAllowedForSite(
        AddFileCommand request,
        FileCategory? category,
        CancellationToken cancellationToken)
    {
        if (!category.HasValue)
        {
            return false;
        }

        var site = await _dbContext.Sites.AsNoTracking()
            .FirstOrDefaultAsync(site => site.Id == request.SiteId, cancellationToken);

        return site is null || site.MediaPolicy.AllowsFileCategory(category.Value);
    }
}
