using AutoMapper;
using Domain.Entities;

namespace Application.Cameras.Queries;

public class CamerasDto
{
    public Guid Id { get; init; }

    public string? Name { get; init; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Camera, CamerasDto>().ForMember(d => d.Name, o => o.MapFrom(s => s.CameraName.Value));
        }
    }
}