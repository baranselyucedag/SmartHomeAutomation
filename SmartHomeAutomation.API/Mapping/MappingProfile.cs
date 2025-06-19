using AutoMapper;
using SmartHomeAutomation.API.DTOs;
using SmartHomeAutomation.Core.Entities;

namespace SmartHomeAutomation.API.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<User, UserDto>();
            CreateMap<RegisterDto, User>();
            CreateMap<UpdateUserDto, User>();

            // Device mappings
            CreateMap<Device, DeviceDto>();
            CreateMap<CreateDeviceDto, Device>();
            CreateMap<UpdateDeviceDto, Device>();

            // Room mappings
            CreateMap<Room, RoomDto>()
                .ForMember(dest => dest.Devices, opt => opt.MapFrom(src => src.Devices));
            CreateMap<CreateRoomDto, Room>();
            CreateMap<UpdateRoomDto, Room>();

            // Scene mappings
            CreateMap<Scene, SceneDto>();
            CreateMap<CreateSceneDto, Scene>();
            CreateMap<UpdateSceneDto, Scene>();

            // SceneDevice mappings
            CreateMap<SceneDevice, SceneDeviceDto>();
            CreateMap<CreateSceneDeviceDto, SceneDevice>();
            CreateMap<UpdateSceneDeviceDto, SceneDevice>();

            // Automation rule mappings
            CreateMap<AutomationRule, AutomationRuleDto>();
            CreateMap<CreateAutomationRuleDto, AutomationRule>();
            CreateMap<UpdateAutomationRuleDto, AutomationRule>();
        }
    }
} 