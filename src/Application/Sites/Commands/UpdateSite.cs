using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Domain.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Sites.Commands;

[Authorize(Roles = UserRoles.Administrator)]
public sealed record UpdateSiteCommand : IRequest
{
    public Guid Id { get; set; }
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string[] MediaCategoriesToAdd { get; init; } = [];
}

public sealed class UpdateSiteValidator : AbstractValidator<UpdateSiteCommand>
{
    public UpdateSiteValidator(IApplicationDbContext dbContext)
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().Length(5, 100);
        RuleFor(command => command.Address).NotEmpty().Length(5, 200);
        RuleFor(command => command.MediaCategoriesToAdd)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Custom((values, context) => MediaCategoryValidation.Validate(values, context));
        RuleFor(command => command.MediaCategoriesToAdd)
            .MustAsync(async (command, values, cancellationToken) =>
            {
                if (values is null)
                {
                    return true;
                }

                var site = await dbContext.Sites
                    .AsNoTracking()
                    .SingleOrDefaultAsync(site => site.Id == command.Id, cancellationToken);

                return site is null
                    || MediaCategoryValidation.GetEffectiveCount(
                        site.MediaPolicy.Categories.Concat(values))
                        <= SiteMediaPolicy.MaxCategoryCount;
            })
            .WithMessage(
                $"A media policy cannot contain more than {SiteMediaPolicy.MaxCategoryCount} categories.");
    }
}

public sealed class UpdateSiteHandler(ISiteService siteService) : IRequestHandler<UpdateSiteCommand>
{
    public Task Handle(UpdateSiteCommand request, CancellationToken cancellationToken) =>
        siteService.UpdateAsync(request, cancellationToken);
}
