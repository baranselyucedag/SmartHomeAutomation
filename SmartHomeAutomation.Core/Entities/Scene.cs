using System.Collections.Generic;

namespace SmartHomeAutomation.Core.Entities
{
    public class Scene : BaseEntity
    {
        public Scene()
        {
            SceneDevices = new List<SceneDevice>();
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public new bool IsActive { get; set; }
        public string Icon { get; set; }
        public int Order { get; set; }

        // Foreign key
        public int UserId { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
        public virtual ICollection<SceneDevice> SceneDevices { get; set; }
    }


} 