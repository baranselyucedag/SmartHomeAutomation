using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartHomeAutomation.API.DTOs;
using SmartHomeAutomation.API.Interfaces;
using SmartHomeAutomation.Core.Entities;
using SmartHomeAutomation.Core.Interfaces;

namespace SmartHomeAutomation.API.Services
{
    public interface IRoomService
    {
        Task<IEnumerable<RoomDto>> GetAllRoomsAsync(int userId);
        Task<RoomDto> GetRoomByIdAsync(int id, int userId);
        Task<RoomDto> CreateRoomAsync(CreateRoomDto createRoomDto, int userId);
        Task<RoomDto> UpdateRoomAsync(int id, UpdateRoomDto updateRoomDto, int userId);
        Task<bool> DeleteRoomAsync(int id, int userId);
        Task<IEnumerable<RoomDto>> GetUserRoomsAsync(int userId);
    }

    public class RoomService : IRoomService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogService _logger;

        public RoomService(IUnitOfWork unitOfWork, IMapper mapper, ILogService logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<RoomDto>> GetAllRoomsAsync(int userId)
        {
            _logger.LogInformation("Getting all rooms for user {UserId}", userId);

            // Sadece kullanıcının kendi odalarını döndür
            var rooms = await _unitOfWork.Rooms.FindAsync(r => r.UserId == userId && r.IsActive);
            
            if (rooms == null)
            {
                _logger.LogWarning("Rooms collection is null");
                return new List<RoomDto>();
            }
            
            var roomsList = rooms.ToList();
            _logger.LogInformation("Retrieved {Count} rooms for user {UserId}", roomsList.Count, userId);
            
            var roomDtos = _mapper.Map<IEnumerable<RoomDto>>(roomsList);
            return roomDtos;
        }

        public async Task<RoomDto> GetRoomByIdAsync(int id, int userId)
        {
            _logger.LogInformation("Getting room {RoomId}", id);

            var room = await _unitOfWork.Rooms.GetByIdAsync(id);

            if (room == null)
            {
                _logger.LogWarning("Room {RoomId} not found", id);
                throw new KeyNotFoundException($"Room with ID {id} not found");
            }

            _logger.LogInformation("Retrieved room {RoomId}", id);
            return _mapper.Map<RoomDto>(room);
        }

        public async Task<RoomDto> CreateRoomAsync(CreateRoomDto createRoomDto, int userId)
        {
            _logger.LogInformation("Creating new room for user {UserId}", userId);

            // Validation
            if (string.IsNullOrWhiteSpace(createRoomDto.Name))
            {
                throw new ArgumentException("Room name is required");
            }

            var room = _mapper.Map<Room>(createRoomDto);
            
            // Kullanıcının ID'sini set et
            room.UserId = userId;
            room.CreatedAt = DateTime.UtcNow;
            room.IsActive = true;
            
            // Ensure Name is not null or empty
            if (string.IsNullOrWhiteSpace(room.Name))
            {
                room.Name = "Yeni Oda";
            }

            _logger.LogInformation("Creating room: Name={Name}, UserId={UserId}", room.Name, room.UserId);

            await _unitOfWork.Rooms.AddAsync(room);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Created room {RoomId} for user {UserId}", room.Id, room.UserId);
            return _mapper.Map<RoomDto>(room);
        }

        public async Task<RoomDto> UpdateRoomAsync(int id, UpdateRoomDto updateRoomDto, int userId)
        {
            _logger.LogInformation("Updating room {RoomId}", id);

            var room = await _unitOfWork.Rooms.GetByIdAsync(id);

            if (room == null)
            {
                _logger.LogWarning("Room {RoomId} not found during update", id);
                throw new KeyNotFoundException($"Room with ID {id} not found");
            }

            _mapper.Map(updateRoomDto, room);
            room.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Rooms.UpdateAsync(room);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Updated room {RoomId}", id);
            return _mapper.Map<RoomDto>(room);
        }

        public async Task<bool> DeleteRoomAsync(int id, int userId)
        {
            _logger.LogInformation("Deleting room {RoomId}", id);

            var room = await _unitOfWork.Rooms.GetByIdAsync(id);

            if (room == null)
            {
                _logger.LogWarning("Room {RoomId} not found during deletion", id);
                throw new KeyNotFoundException($"Room with ID {id} not found");
            }

            room.IsActive = false;
            room.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Rooms.UpdateAsync(room);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Deleted room {RoomId}", id);
            return true;
        }

        public async Task<IEnumerable<RoomDto>> GetUserRoomsAsync(int userId)
        {
            _logger.LogInformation("Getting rooms for user {UserId}", userId);

            var rooms = await _unitOfWork.Rooms.FindAsync(r => r.UserId == userId && r.IsActive);

            _logger.LogInformation("Retrieved {Count} rooms for user {UserId}", rooms.Count(), userId);
            return _mapper.Map<IEnumerable<RoomDto>>(rooms);
        }
    }
} 