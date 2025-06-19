using Microsoft.EntityFrameworkCore;
using SmartHomeAutomation.Core.Entities;
using SmartHomeAutomation.Core.Interfaces;
using SmartHomeAutomation.Infrastructure.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartHomeAutomation.Infrastructure.Repositories
{
    public class RoomRepository : BaseRepository<Room>, IRoomRepository
    {
        public RoomRepository(ApplicationDbContext context) : base(context)
        {
        }
        
        public override async Task<IEnumerable<Room>> GetAllAsync()
        {
            return await _entities.Include(r => r.Devices).ToListAsync();
        }
        
        public override async Task<Room> GetByIdAsync(int id)
        {
            return await _entities.Include(r => r.Devices).FirstOrDefaultAsync(r => r.Id == id);
        }
    }
} 