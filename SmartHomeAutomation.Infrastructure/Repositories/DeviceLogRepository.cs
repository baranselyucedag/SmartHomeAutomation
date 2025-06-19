using Microsoft.EntityFrameworkCore;
using SmartHomeAutomation.Core.Entities;
using SmartHomeAutomation.Core.Interfaces;
using SmartHomeAutomation.Infrastructure.Data;

namespace SmartHomeAutomation.Infrastructure.Repositories
{
    public class DeviceLogRepository : BaseRepository<DeviceLog>, IDeviceLogRepository
    {
        public DeviceLogRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
} 