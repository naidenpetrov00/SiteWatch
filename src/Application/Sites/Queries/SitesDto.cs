using AutoMapper;
using Domain.Entities;

public class SitesDto
{
    public string? Name { get; init; }
    public string? Address { get; init; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Site, SitesDto>().ForMember(d => d.Name, o => o.MapFrom(s => s.Name.Value));
            CreateMap<Site, SitesDto>()
                .ForMember(d => d.Address, o => o.MapFrom(s => s.Address.Value));
        }
    }
}
