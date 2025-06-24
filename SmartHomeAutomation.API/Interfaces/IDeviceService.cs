using System.Collections.Generic;
using System.Threading.Tasks;
using SmartHomeAutomation.API.DTOs;

namespace SmartHomeAutomation.API.Interfaces
{
    public interface IDeviceService
    {
        Task<IEnumerable<DeviceDto>> GetAllDevicesAsync(int userId);
        Task<DeviceDto> GetDeviceByIdAsync(int id);
        Task<DeviceDto> CreateDeviceAsync(CreateDeviceDto createDeviceDto);
        Task<DeviceDto> UpdateDeviceAsync(int id, UpdateDeviceDto updateDeviceDto);
        Task<bool> DeleteDeviceAsync(int id);
        Task<DeviceStatusDto> GetDeviceStatusAsync(int id);
        Task<DeviceStatusDto> UpdateDeviceStatusAsync(int id, DeviceStatusDto statusDto);
        Task<IEnumerable<DeviceDto>> GetDevicesByRoomAsync(int roomId);
        Task<IEnumerable<DeviceDto>> GetDevicesByUserAsync(int userId);
        Task<bool> UpdateDeviceStateAsync(int deviceId, string targetState, string targetValue, int userId);
    }
} 