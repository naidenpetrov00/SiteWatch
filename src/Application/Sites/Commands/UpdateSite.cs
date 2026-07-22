using Application.SeedWork.Interfaces;
using Domain.SeedWork.Enums;
using FluentValidation;
using MediatR;

namespace Application.Sites.Commands;

public sealed record UpdateSiteCommand : IRequest
{
    public Guid Id { get; set; }
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string MediaPolicyPreset { get; init; } = string.Empty;
}

public sealed class UpdateSiteValidator : AbstractValidator<UpdateSiteCommand>
{
    public UpdateSiteValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().Length(5, 100);
        RuleFor(command => command.Address).NotEmpty().Length(5, 200);
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
