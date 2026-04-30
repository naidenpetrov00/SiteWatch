using Application.SeedWork.Interfaces;
using FluentValidation;
using Domain.SeedWork.Enums;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Application.Cameras.Commands;

internal class CreateCameraWithDetailsValidator
    : AbstractValidator<CreateCameraWithDetails>
{
    private readonly IApplicationDbContext _dbContext;

    internal CreateCameraWithDetailsValidator(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;

        RuleFor(cc => cc.CameraName)
            .NotNull()
            .SetValidator(new CameraNameValidator()!);

        RuleFor(cc => cc.CameraBrand)
            .NotNull()
            .SetValidator(new CameraBrandValidator()!);

        RuleFor(cc => cc.Username)
            .MaximumLength(50);

        RuleFor(cc => cc.Password)
            .MaximumLength(50);

        RuleFor(cc => cc.SiteId).NotNull().MustAsync(SiteIdMustExistIfPresented).WithMessage("Site does not exist.");
    }

    private async Task<bool> SiteIdMustExistIfPresented(Guid siteId, CancellationToken cancellationToken) =>
        await _dbContext.Sites.AsNoTracking().AnyAsync(site => site.Id == siteId, cancellationToken: cancellationToken);
}

internal sealed class CameraNameValidator : AbstractValidator<CameraName>
{
    internal CameraNameValidator()
    {
        RuleFor(cn => cn.Value)
            .NotEmpty()
            .Length(1, 100);
    }
}

internal sealed class CameraBrandValidator : AbstractValidator<CameraBrand>
{
    internal CameraBrandValidator()
    {
        RuleFor(cb => cb.Brand)
            .IsInEnum()
            .NotEqual(Brand.Unknown);

        RuleFor(cb => cb.Model)
            .NotEmpty()
            .Length(1, 100);
    }
}
