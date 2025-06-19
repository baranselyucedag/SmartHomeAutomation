using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartHomeAutomation.API.DTOs;
using SmartHomeAutomation.API.Interfaces;
using SmartHomeAutomation.Core.Entities;
using SmartHomeAutomation.Core.Interfaces;

namespace SmartHomeAutomation.API.Services
{
    public interface ISceneService
    {
        Task<List<SceneDto>> GetAllScenesAsync(int userId);
        Task<SceneDto> GetSceneByIdAsync(int sceneId, int userId);
        Task<SceneDto> CreateSceneAsync(CreateSceneDto createSceneDto, int userId);
        Task<SceneDto> UpdateSceneAsync(int sceneId, UpdateSceneDto updateSceneDto, int userId);
        Task DeleteSceneAsync(int sceneId, int userId);
        Task ExecuteSceneAsync(int sceneId, int userId);
        Task<List<DeviceDto>> GetSceneDevicesAsync(int sceneId, int userId);
        Task ScheduleSceneAsync(int sceneId, SceneScheduleDto scheduleDto, int userId);
    }

    public class SceneService : ISceneService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IDeviceService _deviceService;

        public SceneService(IUnitOfWork unitOfWork, IMapper mapper, IDeviceService deviceService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _deviceService = deviceService;
        }

        public async Task<List<SceneDto>> GetAllScenesAsync(int userId)
        {
            var scenes = await _unitOfWork.Repository<Scene>()
                .GetQueryable()
                .Include(s => s.SceneDevices)
                    .ThenInclude(sd => sd.Device)
                .Where(s => s.UserId == userId)
                .ToListAsync();

            return _mapper.Map<List<SceneDto>>(scenes);
        }

        public async Task<SceneDto> GetSceneByIdAsync(int sceneId, int userId)
        {
            var scene = await GetSceneWithDevicesAsync(sceneId, userId);
            return _mapper.Map<SceneDto>(scene);
        }

        public async Task<SceneDto> CreateSceneAsync(CreateSceneDto createSceneDto, int userId)
        {
            var scene = _mapper.Map<Scene>(createSceneDto);
            scene.UserId = userId;

            // Validate that all devices exist and belong to the user
            foreach (var deviceDto in createSceneDto.SceneDevices)
            {
                var device = await _unitOfWork.Repository<Device>()
                    .GetQueryable()
                    .FirstOrDefaultAsync(d => d.Id == deviceDto.DeviceId && d.UserId == userId);

                if (device == null)
                    throw new KeyNotFoundException($"Device with ID {deviceDto.DeviceId} not found");
            }

            await _unitOfWork.Repository<Scene>().AddAsync(scene);
            await _unitOfWork.SaveChangesAsync();

            return await GetSceneByIdAsync(scene.Id, userId);
        }

        public async Task<SceneDto> UpdateSceneAsync(int sceneId, UpdateSceneDto updateSceneDto, int userId)
        {
            var scene = await GetSceneWithDevicesAsync(sceneId, userId);

            _mapper.Map(updateSceneDto, scene);

            // Remove existing scene devices
            _unitOfWork.Repository<SceneDevice>().DeleteRange(scene.SceneDevices);

            // Add new scene devices
            scene.SceneDevices = _mapper.Map<List<SceneDevice>>(updateSceneDto.SceneDevices);
            foreach (var sceneDevice in scene.SceneDevices)
            {
                sceneDevice.SceneId = sceneId;
            }

            await _unitOfWork.SaveChangesAsync();

            return await GetSceneByIdAsync(sceneId, userId);
        }

        public async Task DeleteSceneAsync(int sceneId, int userId)
        {
            var scene = await GetSceneWithDevicesAsync(sceneId, userId);
            _unitOfWork.Repository<Scene>().Delete(scene);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ExecuteSceneAsync(int sceneId, int userId)
        {
            var scene = await GetSceneWithDevicesAsync(sceneId, userId);

            foreach (var sceneDevice in scene.SceneDevices)
            {
                try
                {
                    // Update device state
                    await _deviceService.UpdateDeviceStateAsync(
                        sceneDevice.DeviceId,
                        sceneDevice.TargetState,
                        sceneDevice.TargetValue ?? "",
                        userId);
                }
                catch (Exception ex)
                {
                    // Log the error but continue with other devices
                    // In a real application, you might want to use a proper logging service
                    Console.WriteLine($"Error executing scene for device {sceneDevice.DeviceId}: {ex.Message}");
                }
            }
        }

        public async Task<List<DeviceDto>> GetSceneDevicesAsync(int sceneId, int userId)
        {
            var scene = await GetSceneWithDevicesAsync(sceneId, userId);
            return _mapper.Map<List<DeviceDto>>(scene.SceneDevices.Select(sd => sd.Device));
        }

        public async Task ScheduleSceneAsync(int sceneId, SceneScheduleDto scheduleDto, int userId)
        {
            var scene = await GetSceneWithDevicesAsync(sceneId, userId);

            // Here you would integrate with a scheduling service (e.g., Quartz.NET)
            // For now, we'll just throw a not implemented exception
            throw new NotImplementedException("Scene scheduling is not implemented yet");
        }

        private async Task<Scene> GetSceneWithDevicesAsync(int sceneId, int userId)
        {
            var scene = await _unitOfWork.Repository<Scene>()
                .GetQueryable()
                .Include(s => s.SceneDevices)
                    .ThenInclude(sd => sd.Device)
                .FirstOrDefaultAsync(s => s.Id == sceneId && s.UserId == userId);

            if (scene == null)
                throw new KeyNotFoundException($"Scene with ID {sceneId} not found");

            return scene;
        }
    }
} 