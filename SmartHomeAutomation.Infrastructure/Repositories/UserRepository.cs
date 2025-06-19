using Microsoft.EntityFrameworkCore;
using SmartHomeAutomation.Core.Entities;
using SmartHomeAutomation.Core.Interfaces;
using SmartHomeAutomation.Infrastructure.Data;

namespace SmartHomeAutomation.Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
} 