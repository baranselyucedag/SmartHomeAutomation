using System.Threading.Tasks;
using SmartHomeAutomation.Core.Interfaces;
using SmartHomeAutomation.Infrastructure.Repositories;
using SmartHomeAutomation.Core.Entities;

namespace SmartHomeAutomation.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IUserRepository _users;
        private IDeviceRepository _devices;
        private IRoomRepository _rooms;
        private ISceneRepository _scenes;
        private IDeviceLogRepository _deviceLogs;
        private IAutomationRuleRepository _automationRules;
        private IAutomationLogRepository _automationLogs;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IUserRepository Users => _users ??= new UserRepository(_context);
        public IDeviceRepository Devices => _devices ??= new DeviceRepository(_context);
        public IRoomRepository Rooms => _rooms ??= new RoomRepository(_context);
        public ISceneRepository Scenes => _scenes ??= new SceneRepository(_context);
        public IDeviceLogRepository DeviceLogs => _deviceLogs ??= new DeviceLogRepository(_context);
        public IAutomationRuleRepository AutomationRules => _automationRules ??= new AutomationRuleRepository(_context);
        public IAutomationLogRepository AutomationLogs => _automationLogs ??= new AutomationLogRepository(_context);

        public IRepository<T> Repository<T>() where T : class
        {
            return new BaseRepository<T>(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
} 