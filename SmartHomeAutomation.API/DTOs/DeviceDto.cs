namespace SmartHomeAutomation.API.DTOs
{
    public class DeviceDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public bool IsOnline { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public string FirmwareVersion { get; set; }
        public int RoomId { get; set; }
        public int UserId { get; set; }
    }

    public class CreateDeviceDto
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public string FirmwareVersion { get; set; }
        public int RoomId { get; set; }
        public int UserId { get; set; }
    }

    public class UpdateDeviceDto
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public string FirmwareVersion { get; set; }
        public int? RoomId { get; set; }
    }

    public class DeviceStatusDto
    {
        public string Status { get; set; }
        public bool IsOnline { get; set; }
    }
} 