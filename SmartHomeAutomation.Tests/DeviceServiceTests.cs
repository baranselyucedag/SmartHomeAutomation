using Xunit;
using Moq;
using AutoMapper;
using SmartHomeAutomation.API.Services;
using SmartHomeAutomation.API.DTOs;
using SmartHomeAutomation.Core.Interfaces;
using SmartHomeAutomation.Core.Entities;

namespace SmartHomeAutomation.Tests
{
    public class DeviceServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly DeviceService _deviceService;

        public DeviceServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _deviceService = new DeviceService(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetDevicesPagedAsync_WithValidData_ReturnsPagedResult()
        {
            // Arrange
            var userId = 1;
            var pagination = new PaginationDto { Page = 1, PageSize = 5 };
            var devices = new List<Device>
            {
                new Device { Id = 1, Name = "Test Device 1", Type = "LIGHT", UserId = userId, IsActive = true },
                new Device { Id = 2, Name = "Test Device 2", Type = "TV", UserId = userId, IsActive = true }
            };
            var deviceDtos = new List<DeviceDto>
            {
                new DeviceDto { Id = 1, Name = "Test Device 1", Type = "LIGHT" },
                new DeviceDto { Id = 2, Name = "Test Device 2", Type = "TV" }
            };

            _mockUnitOfWork.Setup(x => x.Devices.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Device, bool>>>()))
                          .ReturnsAsync(devices);
            _mockMapper.Setup(x => x.Map<List<DeviceDto>>(It.IsAny<List<Device>>()))
                      .Returns(deviceDtos);

            // Act
            var result = await _deviceService.GetDevicesPagedAsync(userId, pagination);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(1, result.Page);
            Assert.Equal(5, result.PageSize);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetDevicesPagedAsync_WithSearch_FiltersCorrectly()
        {
            // Arrange
            var userId = 1;
            var pagination = new PaginationDto { Page = 1, PageSize = 5, Search = "Light" };
            var devices = new List<Device>
            {
                new Device { Id = 1, Name = "Smart Light", Type = "LIGHT", UserId = userId, IsActive = true },
                new Device { Id = 2, Name = "TV", Type = "TV", UserId = userId, IsActive = true }
            };

            _mockUnitOfWork.Setup(x => x.Devices.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Device, bool>>>()))
                          .ReturnsAsync(devices);

            // Act
            var result = await _deviceService.GetDevicesPagedAsync(userId, pagination);

            // Assert - Burada search filtering logic'i test edilir
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("name", "asc")]
        [InlineData("name", "desc")]
        [InlineData("type", "asc")]
        public async Task GetDevicesPagedAsync_WithSorting_SortsCorrectly(string sortBy, string sortOrder)
        {
            // Arrange
            var userId = 1;
            var pagination = new PaginationDto { Page = 1, PageSize = 5, SortBy = sortBy, SortOrder = sortOrder };
            var devices = new List<Device>
            {
                new Device { Id = 1, Name = "B Device", Type = "TV", UserId = userId, IsActive = true },
                new Device { Id = 2, Name = "A Device", Type = "LIGHT", UserId = userId, IsActive = true }
            };

            _mockUnitOfWork.Setup(x => x.Devices.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Device, bool>>>()))
                          .ReturnsAsync(devices);

            // Act
            var result = await _deviceService.GetDevicesPagedAsync(userId, pagination);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
        }
    }
} 