using Application.SeedWork.Security;
using Domain.ValueObjects;
using MediatR;

namespace Application.Sites.Queries;

[Authorize(Roles = UserRoles.Administrator)]
public sealed record SiteMediaPolicyPresetsQuery : IRequest<IReadOnlyList<SiteMediaPolicyPresetDto>>;

public sealed class SiteMediaPolicyPresetsQueryHandler
    : IRequestHandler<SiteMediaPolicyPresetsQuery, IReadOnlyList<SiteMediaPolicyPresetDto>>
{
    public Task<IReadOnlyList<SiteMediaPolicyPresetDto>> Handle(
        SiteMediaPolicyPresetsQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<SiteMediaPolicyPresetDto> presets = SiteMediaPolicy.PresetDefinitions
            .Select(definition => new SiteMediaPolicyPresetDto(
                definition.Preset.ToString(),
                definition.DisplayName,
                definition.Categories.ToArray()))
            .ToArray();

        return Task.FromResult(presets);
    }
}

/// <summary>Represents an available site media policy preset.</summary>
public sealed record SiteMediaPolicyPresetDto(
    string Preset,
    string DisplayName,
    IReadOnlyList<string> Categories);
