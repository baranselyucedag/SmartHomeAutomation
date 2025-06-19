using System.Collections.Generic;

namespace SmartHomeAutomation.Core.Entities
{
    public class Device : BaseEntity
    {
        public Device()
        {
            DeviceLogs = new List<DeviceLog>();
            SceneDevices = new List<SceneDevice>();
        }

        public string Name { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public bool IsOnline { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public string FirmwareVersion { get; set; }

        // Foreign keys
        public int RoomId { get; set; }
        public int UserId { get; set; }

        // Navigation properties
        public virtual Room Room { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<DeviceLog> DeviceLogs { get; set; }
        public virtual ICollection<SceneDevice> SceneDevices { get; set; }
    }
} 