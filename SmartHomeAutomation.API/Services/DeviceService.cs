using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using SmartHomeAutomation.API.DTOs;
using SmartHomeAutomation.API.Interfaces;
using SmartHomeAutomation.Core.Entities;
using SmartHomeAutomation.Core.Interfaces;

namespace SmartHomeAutomation.API.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DeviceService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DeviceDto>> GetAllDevicesAsync(int userId)
        {
            // Get only devices that belong to the specific user and are active
            var devices = await _unitOfWork.Devices.FindAsync(d => d.UserId == userId && d.IsActive);
            return _mapper.Map<IEnumerable<DeviceDto>>(devices);
        }

        public async Task<DeviceDto> GetDeviceByIdAsync(int id, int userId)
        {
            var device = await _unitOfWork.Devices.GetByIdAsync(id);
            if (device == null || device.UserId != userId)
            {
                throw new UnauthorizedAccessException("Bu cihaza erişim yetkiniz yok.");
            }
            return _mapper.Map<DeviceDto>(device);
        }

        public async Task<DeviceDto> CreateDeviceAsync(CreateDeviceDto createDeviceDto)
        {
            var device = _mapper.Map<Device>(createDeviceDto);
            device.Status = "OFF";
            device.IsOnline = true;
            device.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Devices.AddAsync(device);
            await _unitOfWork.SaveChangesAsync();

            // Cihaz eklendiğinde log kaydı oluştur
            var deviceLog = new DeviceLog
            {
                DeviceId = device.Id,
                Action = "add",
                OldStatus = "",
                NewStatus = device.Status,
                Timestamp = DateTime.UtcNow,
                Description = "Yeni cihaz eklendi",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.DeviceLogs.AddAsync(deviceLog);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<DeviceDto>(device);
        }

        public async Task<DeviceDto> UpdateDeviceAsync(int id, UpdateDeviceDto updateDeviceDto, int userId)
        {
            var device = await _unitOfWork.Devices.GetByIdAsync(id);
            if (device == null || device.UserId != userId)
            {
                throw new UnauthorizedAccessException("Bu cihaza erişim yetkiniz yok.");
            }

            _mapper.Map(updateDeviceDto, device);
            device.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Devices.UpdateAsync(device);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<DeviceDto>(device);
        }

        public async Task<bool> DeleteDeviceAsync(int id, int userId)
        {
            var device = await _unitOfWork.Devices.GetByIdAsync(id);
            if (device == null || device.UserId != userId)
            {
                return false;
            }

            // Silme yerine pasif hale getir
            device.IsActive = false;
            device.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Devices.UpdateAsync(device);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<DeviceStatusDto> GetDeviceStatusAsync(int id, int userId)
        {
            var device = await _unitOfWork.Devices.GetByIdAsync(id);
            if (device == null || device.UserId != userId)
            {
                throw new UnauthorizedAccessException("Bu cihaza erişim yetkiniz yok.");
            }

            return new DeviceStatusDto
            {
                Status = device.Status,
                IsOnline = device.IsOnline
            };
        }

        public async Task<DeviceStatusDto> UpdateDeviceStatusAsync(int id, DeviceStatusDto statusDto, int userId)
        {
            var device = await _unitOfWork.Devices.GetByIdAsync(id);
            if (device == null || device.UserId != userId)
            {
                throw new UnauthorizedAccessException("Bu cihaza erişim yetkiniz yok.");
            }

            var oldStatus = device.Status;
            device.Status = statusDto.Status;
            device.IsOnline = statusDto.IsOnline;
            device.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Devices.UpdateAsync(device);

            // Durum değiştiğinde log kaydı oluştur
            if (oldStatus != statusDto.Status)
            {
                var deviceLog = new DeviceLog
                {
                    DeviceId = device.Id,
                    Action = "status_change",
                    OldStatus = oldStatus,
                    NewStatus = statusDto.Status,
                    Timestamp = DateTime.UtcNow,
                    Description = $"Cihaz durumu değiştirildi: {oldStatus} -> {statusDto.Status}",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _unitOfWork.DeviceLogs.AddAsync(deviceLog);
            }

            await _unitOfWork.SaveChangesAsync();

            return statusDto;
        }

        public async Task<IEnumerable<DeviceDto>> GetDevicesByRoomAsync(int roomId)
        {
            var devices = await _unitOfWork.Devices.FindAsync(d => d.RoomId == roomId && d.IsActive);
            return _mapper.Map<IEnumerable<DeviceDto>>(devices);
        }

        public async Task<IEnumerable<DeviceDto>> GetDevicesByUserAsync(int userId)
        {
            var devices = await _unitOfWork.Devices.FindAsync(d => d.UserId == userId && d.IsActive);
            return _mapper.Map<IEnumerable<DeviceDto>>(devices);
        }

        public async Task<bool> UpdateDeviceStateAsync(int deviceId, string targetState, string targetValue, int userId)
        {
            var device = await _unitOfWork.Devices.GetByIdAsync(deviceId);
            if (device == null || device.UserId != userId) return false;

            var oldStatus = device.Status;
            device.Status = targetState;
            device.UpdatedAt = DateTime.UtcNow;
            
            // Senaryo çalıştırıldığında cihazı aktif hale getir
            if (!device.IsActive)
            {
                device.IsActive = true;
            }

            await _unitOfWork.Devices.UpdateAsync(device);

            // Durum değiştiğinde log kaydı oluştur
            if (oldStatus != targetState)
            {
                var deviceLog = new DeviceLog
                {
                    DeviceId = device.Id,
                    Action = "status_change",
                    OldStatus = oldStatus,
                    NewStatus = targetState,
                    Timestamp = DateTime.UtcNow,
                    Description = $"Senaryo tarafından cihaz durumu değiştirildi: {oldStatus} -> {targetState}",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _unitOfWork.DeviceLogs.AddAsync(deviceLog);
            }

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
} 