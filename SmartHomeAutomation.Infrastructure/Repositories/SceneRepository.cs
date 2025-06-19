using Microsoft.EntityFrameworkCore;
using SmartHomeAutomation.Core.Entities;
using SmartHomeAutomation.Core.Interfaces;
using SmartHomeAutomation.Infrastructure.Data;

namespace SmartHomeAutomation.Infrastructure.Repositories
{
    public class SceneRepository : BaseRepository<Scene>, ISceneRepository
    {
        public SceneRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
} 