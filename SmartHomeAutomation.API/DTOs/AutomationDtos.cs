namespace SmartHomeAutomation.API.DTOs
{
    public class AutomationRuleDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Condition { get; set; }
        public string Action { get; set; }
        public bool IsEnabled { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string DaysOfWeek { get; set; }
        public int UserId { get; set; }
        public int? DeviceId { get; set; }
        public int? SceneId { get; set; }
    }

    public class CreateAutomationRuleDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Condition { get; set; }
        public string Action { get; set; }
        public bool IsEnabled { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string DaysOfWeek { get; set; }
        public int UserId { get; set; }
        public int? DeviceId { get; set; }
        public int? SceneId { get; set; }
    }

    public class UpdateAutomationRuleDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Condition { get; set; }
        public string Action { get; set; }
        public bool IsEnabled { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string DaysOfWeek { get; set; }
        public int? DeviceId { get; set; }
        public int? SceneId { get; set; }
    }
} 