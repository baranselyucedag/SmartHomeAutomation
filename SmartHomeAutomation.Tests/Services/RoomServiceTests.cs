using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using SmartHomeAutomation.API.DTOs;
using SmartHomeAutomation.API.Mapping;
using SmartHomeAutomation.API.Services;
using SmartHomeAutomation.Core.Entities;
using SmartHomeAutomation.Core.Interfaces;
using Xunit;

namespace SmartHomeAutomation.Tests.Services
{
    public class RoomServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<LogService>> _mockLogger;
        private readonly Mock<IRoomRepository> _mockRoomRepository;
        private readonly IMapper _mapper;
        private readonly IRoomService _roomService;

        public RoomServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<LogService>>();
            _mockRoomRepository = new Mock<IRoomRepository>();
            
            // UnitOfWork'ün Rooms özelliğini mock et
            _mockUnitOfWork.Setup(uow => uow.Rooms).Returns(_mockRoomRepository.Object);
            
            // AutoMapper konfigürasyonu
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            });
            _mapper = mapperConfig.CreateMapper();
            
            // Test edilecek servis
            _roomService = new RoomService(_mockUnitOfWork.Object, _mapper, new LogService(_mockLogger.Object));
        }

        [Fact]
        public async Task GetRoomByIdAsync_ExistingRoom_ReturnsRoomDto()
        {
            // Arrange
            int roomId = 1;
            int userId = 1;
            
            var room = new Room
            {
                Id = roomId,
                Name = "Test Room",
                Description = "Test Description",
                UserId = userId,
                Devices = new List<Device>()
            };
            
            // GetByIdAsync metodunu mock et
            _mockRoomRepository.Setup(repo => repo.GetByIdAsync(roomId))
                .ReturnsAsync(room);
            
            // Act
            var result = await _roomService.GetRoomByIdAsync(roomId, userId);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(roomId, result.Id);
            Assert.Equal("Test Room", result.Name);
            Assert.Equal("Test Description", result.Description);
        }
        
        [Fact]
        public async Task GetRoomByIdAsync_NonExistingRoom_ThrowsKeyNotFoundException()
        {
            // Arrange
            int roomId = 999;
            int userId = 1;
            
            // GetByIdAsync metodunu null dönecek şekilde mock et
            _mockRoomRepository.Setup(repo => repo.GetByIdAsync(roomId))
                .ReturnsAsync((Room)null);
            
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _roomService.GetRoomByIdAsync(roomId, userId));
        }
        
        [Fact]
        public async Task GetPaginatedRoomsAsync_ReturnsCorrectPagination()
        {
            // Arrange
            int userId = 1;
            var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 2 };
            
            var rooms = new List<Room>
            {
                new Room { Id = 1, Name = "Room 1", UserId = userId },
                new Room { Id = 2, Name = "Room 2", UserId = userId },
                new Room { Id = 3, Name = "Room 3", UserId = userId },
                new Room { Id = 4, Name = "Room 4", UserId = userId },
                new Room { Id = 5, Name = "Room 5", UserId = userId }
            };
            
            _mockRoomRepository.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<Room, bool>>>()))
                .ReturnsAsync(rooms);
            
            // Act
            var result = await _roomService.GetPaginatedRoomsAsync(userId, paginationParams);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(3, result.TotalPages);
            Assert.Equal(2, result.Items.Count());
        }
    }
} 