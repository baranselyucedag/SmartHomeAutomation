using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SmartHomeAutomation.Core.Entities;

namespace SmartHomeAutomation.Core.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        IQueryable<T> GetQueryable();
        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entities);
    }

    public interface IUserRepository : IRepository<User> { }
    public interface IDeviceRepository : IRepository<Device> { }
    public interface IRoomRepository : IRepository<Room> { }
    public interface ISceneRepository : IRepository<Scene> { }
    public interface IDeviceLogRepository : IRepository<DeviceLog> { }
    public interface IAutomationRuleRepository : IRepository<AutomationRule> { }
    public interface IAutomationLogRepository : IRepository<AutomationLog> { }
} 