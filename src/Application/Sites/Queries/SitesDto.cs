using AutoMapper;
using Domain.Entities;
using Domain.ValueObjects;

namespace Application.Sites.Queries;

public class SitesDto
{
    public Guid Id { get; init; }

    public string? Name { get; init; }
    public string? Address { get; init; }
    public string ManagerId { get; init; } = string.Empty;
    public string ManagerDisplayName { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public SiteMediaPolicyDto MediaPolicy { get; init; } = null!;

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Site, SitesDto>()
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name.Value))
                .ForMember(d => d.Address, o => o.MapFrom(s => s.Address.Value))
                .ForMember(d => d.ManagerDisplayName, o => o.MapFrom(s => s.Manager.UserName ?? s.Manager.Email ?? string.Empty))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.MediaPolicy, o => o.MapFrom(s => s.MediaPolicy));

            CreateMap<SiteMediaPolicy, SiteMediaPolicyDto>()
                .ForMember(d => d.Preset, o => o.MapFrom(s => s.Preset.ToString()))
                .ForMember(
                    d => d.AllowedImageCategories,
                    o => o.MapFrom(s => s.AllowedImageCategories.Select(category => category.ToString()).ToArray())
                )
                .ForMember(
                    d => d.AllowedVideoCategories,
                    o => o.MapFrom(s => s.AllowedVideoCategories.Select(category => category.ToString()).ToArray())
                );
        }
    }
}

public sealed class SiteMediaPolicyDto
{
    public string Preset { get; init; } = string.Empty;
    public string[] AllowedImageCategories { get; init; } = [];
    public string[] AllowedVideoCategories { get; init; } = [];
}
