using System.Collections.Generic;
using System.Threading.Tasks;
using SmartHomeAutomation.API.DTOs;

namespace SmartHomeAutomation.API.Interfaces
{
    public interface IDeviceService
    {
        Task<IEnumerable<DeviceDto>> GetAllDevicesAsync(int userId);
        Task<DeviceDto> GetDeviceByIdAsync(int id, int userId);
        Task<DeviceDto> CreateDeviceAsync(CreateDeviceDto createDeviceDto);
        Task<DeviceDto> UpdateDeviceAsync(int id, UpdateDeviceDto updateDeviceDto, int userId);
        Task<bool> DeleteDeviceAsync(int id, int userId);
        Task<DeviceStatusDto> GetDeviceStatusAsync(int id, int userId);
        Task<DeviceStatusDto> UpdateDeviceStatusAsync(int id, DeviceStatusDto statusDto, int userId);
        Task<IEnumerable<DeviceDto>> GetDevicesByRoomAsync(int roomId);
        Task<IEnumerable<DeviceDto>> GetDevicesByUserAsync(int userId);
        Task<bool> UpdateDeviceStateAsync(int deviceId, string targetState, string targetValue, int userId);
    }
} 