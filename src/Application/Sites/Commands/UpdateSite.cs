using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Domain.SeedWork.Enums;
using FluentValidation;
using MediatR;

namespace Application.Sites.Commands;

[Authorize(Roles = UserRoles.Administrator)]
public sealed record UpdateSiteCommand : IRequest
{
    public Guid Id { get; set; }
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string ManagerId { get; init; } = string.Empty;
    public string StartDate { get; init; } = string.Empty;
    public string? EndDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public string MediaPolicyPreset { get; init; } = string.Empty;
}

public sealed class UpdateSiteValidator : AbstractValidator<UpdateSiteCommand>
{
    public UpdateSiteValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().Length(5, 100);
        RuleFor(command => command.Address).NotEmpty().Length(5, 200);
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
            .Must(value =>
                Enum.TryParse<MediaPolicyPreset>(value, true, out var preset)
                && Enum.IsDefined(typeof(MediaPolicyPreset), preset))
            .WithMessage("MediaPolicyPreset must be a valid media policy preset.");
    }
}

public sealed class UpdateSiteHandler(ISiteService siteService) : IRequestHandler<UpdateSiteCommand>
{
    public Task Handle(UpdateSiteCommand request, CancellationToken cancellationToken) =>
        siteService.UpdateAsync(request, cancellationToken);
}
