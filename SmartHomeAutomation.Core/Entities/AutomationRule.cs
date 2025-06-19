using System;

namespace SmartHomeAutomation.Core.Entities
{
    public class AutomationRule : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Condition { get; set; }
        public string Action { get; set; }
        public bool IsEnabled { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string DaysOfWeek { get; set; }

        // Foreign keys
        public int UserId { get; set; }
        public int? DeviceId { get; set; }
        public int? SceneId { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
        public virtual Device Device { get; set; }
        public virtual Scene Scene { get; set; }
    }
} 