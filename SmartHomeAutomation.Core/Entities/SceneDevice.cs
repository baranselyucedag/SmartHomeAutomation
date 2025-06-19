using System;

namespace SmartHomeAutomation.Core.Entities
{
    public class SceneDevice : BaseEntity
    {
        public int SceneId { get; set; }
        public Scene Scene { get; set; }
        public int DeviceId { get; set; }
        public Device Device { get; set; }
        public string TargetState { get; set; }
        public string TargetValue { get; set; } // For dimmable devices, temperature settings etc.
        public int Order { get; set; }
    }
} 