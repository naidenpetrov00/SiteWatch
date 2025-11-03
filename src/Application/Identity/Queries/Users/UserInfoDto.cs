using Application.SeedWork.Models;
using AutoMapper;

namespace Application.Identity.Queries.Users;

public record UserInfoDto
{
    public string? Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ApplicationUser, UserInfoDto>();
        }
    }
}
