using Application.SeedWork.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Cameras.Queries;

public class CamerasBySiteQueryValidator : AbstractValidator<CamerasBySiteQuery>
{
    private readonly IApplicationDbContext _dbContext;

    public CamerasBySiteQueryValidator(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        RuleFor(sq => sq.SiteId)
            .NotEmpty()
            .WithMessage("SiteId is required.")
            .MustAsync(SiteExists)
            .WithMessage("Site not found.");
    }

    private async Task<bool> SiteExists(Guid siteId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _dbContext.Sites.AnyAsync(site => site.Id == siteId, cancellationToken);
    }
}