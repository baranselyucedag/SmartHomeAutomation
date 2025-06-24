using System.Collections.Generic;

namespace SmartHomeAutomation.Core.Entities
{
    public class Room : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Floor { get; set; }
        
        // Navigation properties
        public ICollection<Device> Devices { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }

        public Room()
        {
            Devices = new List<Device>();
        }
    }
} 