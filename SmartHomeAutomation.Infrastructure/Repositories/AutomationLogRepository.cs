using Microsoft.EntityFrameworkCore;
using SmartHomeAutomation.Core.Entities;
using SmartHomeAutomation.Core.Interfaces;
using SmartHomeAutomation.Infrastructure.Data;

namespace SmartHomeAutomation.Infrastructure.Repositories
{
    public class AutomationLogRepository : BaseRepository<AutomationLog>, IAutomationLogRepository
    {
        public AutomationLogRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
} 