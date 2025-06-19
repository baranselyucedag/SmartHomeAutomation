using Microsoft.EntityFrameworkCore;
using SmartHomeAutomation.Core.Entities;
using SmartHomeAutomation.Core.Interfaces;
using SmartHomeAutomation.Infrastructure.Data;

namespace SmartHomeAutomation.Infrastructure.Repositories
{
    public class AutomationRuleRepository : BaseRepository<AutomationRule>, IAutomationRuleRepository
    {
        public AutomationRuleRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
} 