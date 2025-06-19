using System;

namespace SmartHomeAutomation.Core.Entities
{
    public class AutomationLog : BaseEntity
    {
        public DateTime ExecutionTime { get; set; }
        public bool IsSuccess { get; set; }
        public string Result { get; set; }
        public string ErrorMessage { get; set; }

        // Foreign key
        public int AutomationRuleId { get; set; }

        // Navigation property
        public virtual AutomationRule AutomationRule { get; set; }
    }
} 