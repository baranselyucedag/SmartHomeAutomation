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

        // Senaryo şablonları - karmaşık if-else blokları yerine temiz veri yapısı
        private readonly Dictionary<string, SceneTemplate> _sceneTemplates = new()
        {
            ["Film Gecesi"] = new SceneTemplate
            {
                Name = "Film Gecesi",
                Description = "Film izlemek için ideal ortam",
                Icon = "🎬",
                DeviceRules = new()
                {
                    ["LIGHT"] = new DeviceRule { State = "OFF", Value = null },
                    ["TV"] = new DeviceRule { State = "ON", Value = "75" },
                    ["THERMOSTAT"] = new DeviceRule { State = "ON", Value = "21" }
                }
            },
            ["Romantik Akşam"] = new SceneTemplate
            {
                Name = "Romantik Akşam",
                Description = "Romantik bir akşam için loş ışık",
                Icon = "❤️",
                DeviceRules = new()
                {
                    ["LIGHT"] = new DeviceRule { State = "ON", Value = "15" },
                    ["TV"] = new DeviceRule { State = "OFF", Value = null }
                }
            },
            ["Çalışma Modu"] = new SceneTemplate
            {
                Name = "Çalışma Modu",
                Description = "Verimli çalışma için parlak ışık",
                Icon = "💼",
                DeviceRules = new()
                {
                    ["LIGHT"] = new DeviceRule { State = "ON", Value = "100" },
                    ["TV"] = new DeviceRule { State = "OFF", Value = null },
                    ["THERMOSTAT"] = new DeviceRule { State = "ON", Value = "22" }
                }
            },
            ["Uyku Zamanı"] = new SceneTemplate
            {
                Name = "Uyku Zamanı",
                Description = "Uyumaya hazırlık - tüm cihazlar kapalı",
                Icon = "🌙",
                DeviceRules = new()
                {
                    ["LIGHT"] = new DeviceRule { State = "OFF", Value = null },
                    ["TV"] = new DeviceRule { State = "OFF", Value = null },
                    ["THERMOSTAT"] = new DeviceRule { State = "ON", Value = "19" }
                }
            },
            ["Günaydın"] = new SceneTemplate
            {
                Name = "Günaydın",
                Description = "Güne başlamak için ideal ayarlar",
                Icon = "☀️",
                DeviceRules = new()
                {
                    ["LIGHT"] = new DeviceRule { State = "ON", Value = "80" },
                    ["THERMOSTAT"] = new DeviceRule { State = "ON", Value = "23" }
                }
            },
            ["Parti Modu"] = new SceneTemplate
            {
                Name = "Parti Modu",
                Description = "Parti için renkli ışıklar",
                Icon = "🎉",
                DeviceRules = new()
                {
                    ["LIGHT"] = new DeviceRule { State = "ON", Value = "85" },
                    ["TV"] = new DeviceRule { State = "ON", Value = "60" }
                }
            },
            ["Okuma Saati"] = new SceneTemplate
            {
                Name = "Okuma Saati",
                Description = "Rahat okuma için uygun ışık",
                Icon = "📚",
                DeviceRules = new()
                {
                    ["LIGHT"] = new DeviceRule { State = "ON", Value = "65" },
                    ["TV"] = new DeviceRule { State = "OFF", Value = null }
                }
            },
            ["Spor Zamanı"] = new SceneTemplate
            {
                Name = "Spor Zamanı",
                Description = "Egzersiz için enerji verici ışık",
                Icon = "💪",
                DeviceRules = new()
                {
                    ["LIGHT"] = new DeviceRule { State = "ON", Value = "95" },
                    ["TV"] = new DeviceRule { State = "OFF", Value = null },
                    ["THERMOSTAT"] = new DeviceRule { State = "ON", Value = "20" }
                }
            }
        };

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

            // Validate devices
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
            var existingScene = await _unitOfWork.Repository<Scene>()
                .GetQueryable()
                .FirstOrDefaultAsync(s => s.Id == sceneId && s.UserId == userId);

            if (existingScene == null)
                throw new KeyNotFoundException($"Scene with ID {sceneId} not found");

            // Update scene properties
            existingScene.Name = updateSceneDto.Name;
            existingScene.Description = updateSceneDto.Description;
            existingScene.IsActive = updateSceneDto.IsActive;
            existingScene.Icon = updateSceneDto.Icon;
            existingScene.Order = updateSceneDto.Order;
            existingScene.UpdatedAt = DateTime.UtcNow;

            // Clear and rebuild scene devices
            await ClearSceneDevicesAsync(sceneId);
            await AddSceneDevicesAsync(sceneId, updateSceneDto.SceneDevices, userId);

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

            var tasks = scene.SceneDevices.Select(async sceneDevice =>
            {
                try
                {
                    await _deviceService.UpdateDeviceStateAsync(
                        sceneDevice.DeviceId,
                        sceneDevice.TargetState,
                        sceneDevice.TargetValue ?? "",
                        userId);
                }
                catch (Exception ex)
                {
                    // Log error silently - could be logged to a proper logging system
                }
            });

            await Task.WhenAll(tasks);
        }

        public async Task<List<DeviceDto>> GetSceneDevicesAsync(int sceneId, int userId)
        {
            var scene = await GetSceneWithDevicesAsync(sceneId, userId);
            return _mapper.Map<List<DeviceDto>>(scene.SceneDevices.Select(sd => sd.Device));
        }

        public async Task ScheduleSceneAsync(int sceneId, SceneScheduleDto scheduleDto, int userId)
        {
            var scene = await GetSceneWithDevicesAsync(sceneId, userId);
            throw new NotImplementedException("Scene scheduling is not implemented yet");
        }



        // Helper methods - temiz ve basit
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

        private async Task ClearSceneDevicesAsync(int sceneId)
        {
            var existingSceneDevices = await _unitOfWork.Repository<SceneDevice>()
                .GetQueryable()
                .Where(sd => sd.SceneId == sceneId)
                .ToListAsync();

            foreach (var sceneDevice in existingSceneDevices)
            {
                _unitOfWork.Repository<SceneDevice>().Delete(sceneDevice);
            }
        }

        private async Task AddSceneDevicesAsync(int sceneId, ICollection<UpdateSceneDeviceDto> deviceDtos, int userId)
        {
            if (deviceDtos == null || !deviceDtos.Any())
                return;

            foreach (var deviceDto in deviceDtos)
            {
                var deviceExists = await _unitOfWork.Repository<Device>()
                    .GetQueryable()
                    .AnyAsync(d => d.Id == deviceDto.DeviceId && d.UserId == userId);

                if (!deviceExists)
                    throw new KeyNotFoundException($"Device with ID {deviceDto.DeviceId} not found");

                var sceneDevice = new SceneDevice
                {
                    SceneId = sceneId,
                    DeviceId = deviceDto.DeviceId,
                    TargetState = deviceDto.TargetState,
                    TargetValue = deviceDto.TargetValue,
                    Order = deviceDto.Order,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _unitOfWork.Repository<SceneDevice>().AddAsync(sceneDevice);
            }
        }

        private async Task ClearAllUserScenesAsync(int userId)
        {
            var existingScenes = await _unitOfWork.Repository<Scene>()
                .GetQueryable()
                .Where(s => s.UserId == userId)
                .ToListAsync();

            foreach (var scene in existingScenes)
            {
                _unitOfWork.Repository<Scene>().Delete(scene);
            }
        }

        private async Task CreateSceneFromTemplate(SceneTemplate template, List<Device> devices, int userId, int order)
        {
            var scene = new Scene
            {
                Name = template.Name,
                Description = template.Description,
                Icon = template.Icon,
                IsActive = true,
                Order = order,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Scene>().AddAsync(scene);
            await _unitOfWork.SaveChangesAsync(); // Get scene ID

            // Add devices based on template rules
            var deviceOrder = 1;
            foreach (var device in devices)
            {
                if (template.DeviceRules.TryGetValue(device.Type, out var rule))
                {
                    var sceneDevice = new SceneDevice
                    {
                        SceneId = scene.Id,
                        DeviceId = device.Id,
                        TargetState = rule.State,
                        TargetValue = rule.Value,
                        Order = deviceOrder++,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    await _unitOfWork.Repository<SceneDevice>().AddAsync(sceneDevice);
                }
            }
            
            // Save scene devices
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task ApplySceneTemplate(Scene scene, SceneTemplate template, int userId)
        {
            // Clear existing scene devices
            await ClearSceneDevicesAsync(scene.Id);

            // Get user devices (include inactive devices for scene creation)
            var devices = await _unitOfWork.Repository<Device>()
                .GetQueryable()
                .Where(d => d.UserId == userId)
                .ToListAsync();

            // Apply template rules
            var deviceOrder = 1;
            foreach (var device in devices)
            {
                if (template.DeviceRules.TryGetValue(device.Type, out var rule))
                {
                    var sceneDevice = new SceneDevice
                    {
                        SceneId = scene.Id,
                        DeviceId = device.Id,
                        TargetState = rule.State,
                        TargetValue = rule.Value,
                        Order = deviceOrder++,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    await _unitOfWork.Repository<SceneDevice>().AddAsync(sceneDevice);
                }
            }
            
            // Save scene devices
            await _unitOfWork.SaveChangesAsync();
        }
    }

    // Temiz veri yapıları - if-else bloklarının yerine
    public class SceneTemplate
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Icon { get; set; } = "";
        public Dictionary<string, DeviceRule> DeviceRules { get; set; } = new();
    }

    public class DeviceRule
    {
        public string State { get; set; } = "";
        public string? Value { get; set; }
    }
} 