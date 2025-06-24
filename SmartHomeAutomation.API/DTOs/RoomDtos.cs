using System.Collections.Generic;

namespace SmartHomeAutomation.API.DTOs
{
    public class RoomDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Floor { get; set; }
        public List<DeviceDto> Devices { get; set; } = new List<DeviceDto>();
    }

    public class CreateRoomDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Floor { get; set; }
        // UserId removed - will be set in service layer
    }

    public class UpdateRoomDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Floor { get; set; }
    }
} 