using AutoMapper;
using Domain.Entities;
using Domain.ValueObjects;

namespace Application.Sites.Queries;

public class SitesDto
{
    public Guid Id { get; init; }

    public string? Name { get; init; }
    public string? Address { get; init; }
    public SiteMediaPolicyDto MediaPolicy { get; init; } = null!;

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Site, SitesDto>()
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name.Value))
                .ForMember(d => d.Address, o => o.MapFrom(s => s.Address.Value))
                .ForMember(d => d.MediaPolicy, o => o.MapFrom(s => s.MediaPolicy));

            CreateMap<SiteMediaPolicy, SiteMediaPolicyDto>()
                .ForMember(d => d.Preset, o => o.MapFrom(s => s.Preset.ToString()))
                .ForMember(d => d.Categories, o => o.MapFrom(s => s.Categories.ToArray()));
        }
    }
}

public sealed class SiteMediaPolicyDto
{
    public string Preset { get; init; } = string.Empty;
    public string[] Categories { get; init; } = [];

    public static SiteMediaPolicyDto From(SiteMediaPolicy policy) => new()
    {
        Preset = policy.Preset.ToString(),
        Categories = policy.Categories.ToArray(),
    };
}
