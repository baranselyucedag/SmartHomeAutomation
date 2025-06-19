using System.Collections.Generic;

namespace SmartHomeAutomation.API.DTOs
{
    public class SceneDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public string Icon { get; set; }
        public int Order { get; set; }
        public int UserId { get; set; }
        public ICollection<SceneDeviceDto> SceneDevices { get; set; }
    }

    public class CreateSceneDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public int Order { get; set; }
        public int UserId { get; set; }
        public ICollection<CreateSceneDeviceDto> SceneDevices { get; set; }
    }

    public class UpdateSceneDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public string Icon { get; set; }
        public int Order { get; set; }
        public ICollection<UpdateSceneDeviceDto> SceneDevices { get; set; }
    }

    public class SceneDeviceDto
    {
        public int Id { get; set; }
        public string TargetState { get; set; }
        public int Order { get; set; }
        public int SceneId { get; set; }
        public int DeviceId { get; set; }
        public DeviceDto Device { get; set; }
    }

    public class CreateSceneDeviceDto
    {
        public string TargetState { get; set; }
        public int Order { get; set; }
        public int DeviceId { get; set; }
    }

    public class UpdateSceneDeviceDto
    {
        public string TargetState { get; set; }
        public int Order { get; set; }
        public int DeviceId { get; set; }
    }

    public class SceneScheduleDto
    {
        public string CronExpression { get; set; }
        public bool IsEnabled { get; set; }
    }
} 