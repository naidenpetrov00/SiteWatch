using Application.SeedWork.Interfaces;
using FluentValidation;
using MediatR;

namespace Application.Sites.Commands;

public sealed record CreateSiteCommand : IRequest<Guid>
{
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string MediaPolicyPreset { get; init; } = string.Empty;
}

public sealed class CreateSiteValidator : AbstractValidator<CreateSiteCommand>
{
    public CreateSiteValidator()
    {
        RuleFor(command => command.Name).NotEmpty().Length(5, 100);
        RuleFor(command => command.Address).NotEmpty().Length(5, 200);
        RuleFor(command => command.MediaPolicyPreset)
            .Must(value => Enum.TryParse<Domain.SeedWork.Enums.MediaPolicyPreset>(value, true, out _))
            .WithMessage("MediaPolicyPreset must be a valid media policy preset.");
    }
}

public sealed class CreateSiteHandler(ISiteService siteService)
    : IRequestHandler<CreateSiteCommand, Guid>
{
    public Task<Guid> Handle(CreateSiteCommand request, CancellationToken cancellationToken) =>
        siteService.CreateAsync(request, cancellationToken);
}
