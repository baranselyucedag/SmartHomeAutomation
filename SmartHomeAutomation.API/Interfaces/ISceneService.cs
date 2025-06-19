using System.Collections.Generic;
using System.Threading.Tasks;
using SmartHomeAutomation.API.DTOs;

namespace SmartHomeAutomation.API.Interfaces
{
    public interface ISceneService
    {
        Task<IEnumerable<SceneDto>> GetAllScenesAsync();
        Task<SceneDto> GetSceneByIdAsync(int id);
        Task<SceneDto> CreateSceneAsync(CreateSceneDto createSceneDto);
        Task<SceneDto> UpdateSceneAsync(int id, UpdateSceneDto updateSceneDto);
        Task<bool> DeleteSceneAsync(int id);
        Task<IEnumerable<SceneDto>> GetScenesByUserAsync(int userId);
        Task<bool> ExecuteSceneAsync(int id);
        Task<IEnumerable<DeviceDto>> GetSceneDevicesAsync(int sceneId);
    }
} 