using System.Collections.Generic;
using System.Threading.Tasks;
using SmartHomeAutomation.API.DTOs;

namespace SmartHomeAutomation.API.Interfaces
{
    public interface IAutomationService
    {
        Task<IEnumerable<AutomationRuleDto>> GetAllRulesAsync();
        Task<AutomationRuleDto> GetRuleByIdAsync(int id);
        Task<AutomationRuleDto> CreateRuleAsync(CreateAutomationRuleDto createRuleDto);
        Task<AutomationRuleDto> UpdateRuleAsync(int id, UpdateAutomationRuleDto updateRuleDto);
        Task<bool> DeleteRuleAsync(int id);
        Task<bool> ToggleRuleAsync(int id, bool isEnabled);
        Task<IEnumerable<AutomationRuleDto>> GetUserRulesAsync(int userId);
    }
} 