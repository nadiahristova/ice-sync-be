using AutoMapper;
using IceSync.Domain.Dtos;
using IceSync.Domain.Entities;
using IceSync.Domain.Settings;

namespace IceSync.API.Profiles;

public class MapperProfile : Profile
{
	public MapperProfile()
	{
        CreateMap<WorkflowDto, Workflow>()
            .ForMember(dest => dest.WorkflowId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.WorkflowName, opt => opt.MapFrom(src => src.Name))
            .ReverseMap()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<UniversalLoaderLoginDto, UniversalLoaderSettings>()
            .ReverseMap();
    }
}
