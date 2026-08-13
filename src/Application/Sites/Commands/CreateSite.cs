using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using FluentValidation;
using MediatR;
using Domain.SeedWork.Enums;
using Domain.ValueObjects;

namespace Application.Sites.Commands;

[Authorize(Roles = UserRoles.Administrator)]
public sealed record CreateSiteCommand : IRequest<Guid>
{
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string ManagerId { get; init; } = string.Empty;
    public string StartDate { get; init; } = string.Empty;
    public string? EndDate { get; init; }
    public string Status { get; init; } = SiteStatus.Planning.ToString();
    public string MediaPolicyPreset { get; init; } = string.Empty;
    public string[] MediaCategories { get; init; } = [];
}

public sealed class CreateSiteValidator : AbstractValidator<CreateSiteCommand>
{
    public CreateSiteValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Name must not be empty.")
            .Length(5, 100);
        RuleFor(command => command.Address)
            .NotEmpty()
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Address must not be empty.")
            .Length(5, 200);
        RuleFor(command => command.ManagerId).NotEmpty();
        RuleFor(command => command.StartDate)
            .Must(value => DateOnly.TryParse(value, out _))
            .WithMessage("StartDate must be a valid date.");
        RuleFor(command => command.EndDate)
            .Must(value => string.IsNullOrWhiteSpace(value) || DateOnly.TryParse(value, out _))
            .WithMessage("EndDate must be a valid date.");
        RuleFor(command => command)
            .Must(command =>
                !DateOnly.TryParse(command.StartDate, out var startDate)
                || string.IsNullOrWhiteSpace(command.EndDate)
                || !DateOnly.TryParse(command.EndDate, out var endDate)
                || endDate >= startDate)
            .WithMessage("EndDate cannot be before StartDate.");
        RuleFor(command => command.Status)
            .Must(value => Enum.TryParse<SiteStatus>(value, true, out var status)
                && Enum.IsDefined(typeof(SiteStatus), status))
            .WithMessage("Status must be a valid site status.");
        RuleFor(command => command.MediaPolicyPreset)
            .Must(value => SiteMediaPolicy.TryParsePreset(value, out _))
            .WithMessage("MediaPolicyPreset must be a valid media policy preset.");
        RuleFor(command => command.MediaCategories)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Custom((values, context) => MediaCategoryValidation.Validate(values, context));
    }
}

public sealed class CreateSiteHandler(ISiteService siteService)
    : IRequestHandler<CreateSiteCommand, Guid>
{
    public Task<Guid> Handle(CreateSiteCommand request, CancellationToken cancellationToken) =>
        siteService.CreateAsync(request, cancellationToken);
}
