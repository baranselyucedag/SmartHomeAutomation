using System.Collections.Generic;

namespace SmartHomeAutomation.Core.Entities
{
    public class User : BaseEntity
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }

        // Navigation properties
        public virtual ICollection<Room> Rooms { get; set; }
        public virtual ICollection<Device> Devices { get; set; }
        public virtual ICollection<Scene> Scenes { get; set; }
    }
} 