using System.Threading.Tasks;

namespace SmartHomeAutomation.Core.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        IDeviceRepository Devices { get; }
        IRoomRepository Rooms { get; }
        ISceneRepository Scenes { get; }
        IDeviceLogRepository DeviceLogs { get; }
        IAutomationRuleRepository AutomationRules { get; }
        IAutomationLogRepository AutomationLogs { get; }
        IRepository<T> Repository<T>() where T : class;
        Task<int> SaveChangesAsync();
    }
} 