using System;

namespace SmartHomeAutomation.Core.Entities
{
    public class DeviceLog : BaseEntity
    {
        public string Action { get; set; }
        public string OldStatus { get; set; }
        public string NewStatus { get; set; }
        public DateTime Timestamp { get; set; }
        public string Description { get; set; }

        // Foreign key
        public int DeviceId { get; set; }

        // Navigation property
        public virtual Device Device { get; set; }
    }
} 