using AutoMapper;
using Domain.Entities;

namespace Application.Cameras.Queries;

public class CameraDto
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? Brand { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }
    public string? IpAddress { get; init; }
    public int Port { get; init; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Camera, CameraDto>().ForMember(d => d.Name, o => o.MapFrom(s => s.CameraName.Value))
                .ForMember(cDto => cDto.Brand, o => o.MapFrom(c => c.CameraBrand.Brand.ToString()));
        }
    }
}