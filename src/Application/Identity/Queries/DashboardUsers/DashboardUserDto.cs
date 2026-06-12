using AutoMapper;
using Domain.Entities;

namespace Application.Identity.Queries.DashboardUsers;

public record DashboardUserDto
{
    public string? Id { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public bool IsPhoneNumberConfirmed { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ApplicationUser, DashboardUserDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.IsEmailConfirmed, opt => opt.MapFrom(src => src.EmailConfirmed))
                .ForMember(
                    dest => dest.IsPhoneNumberConfirmed,
                    opt => opt.MapFrom(src => src.PhoneNumberConfirmed)
                );
        }
    }
}
