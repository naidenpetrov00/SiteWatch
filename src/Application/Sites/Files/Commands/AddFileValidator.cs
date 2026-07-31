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

}
