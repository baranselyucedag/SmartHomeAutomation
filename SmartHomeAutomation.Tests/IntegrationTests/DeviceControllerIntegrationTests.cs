using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SmartHomeAutomation.Infrastructure.Data;
using SmartHomeAutomation.Core.Entities;
using SmartHomeAutomation.API.DTOs;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace SmartHomeAutomation.Tests.IntegrationTests
{
    public class DeviceControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public DeviceControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the real database
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Add in-memory database
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb");
                    });

                    // Add test authentication
                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
                });
            });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetAllDevices_ReturnsSuccessStatusCode()
        {
            // Arrange
            await SeedTestDataAsync();

            // Act
            var response = await _client.GetAsync("/api/device");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
        }

        [Fact]
        public async Task CreateDevice_WithValidData_ReturnsCreatedDevice()
        {
            // Arrange
            await SeedTestDataAsync();
            var createDeviceDto = new CreateDeviceDto
            {
                Name = "Test Light",
                Type = "LIGHT",
                RoomId = 1,
                UserId = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/device", createDeviceDto);

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var device = JsonSerializer.Deserialize<DeviceDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            Assert.NotNull(device);
            Assert.Equal("Test Light", device.Name);
            Assert.Equal("LIGHT", device.Type);
        }

        [Fact]
        public async Task GetDevice_WithValidId_ReturnsDevice()
        {
            // Arrange
            await SeedTestDataAsync();

            // Act
            var response = await _client.GetAsync("/api/device/1");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var device = JsonSerializer.Deserialize<DeviceDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            Assert.NotNull(device);
            Assert.Equal(1, device.Id);
        }

        [Fact]
        public async Task UpdateDevice_WithValidData_ReturnsUpdatedDevice()
        {
            // Arrange
            await SeedTestDataAsync();
            var updateDeviceDto = new UpdateDeviceDto
            {
                Name = "Updated Light",
                Type = "LIGHT"
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/device/1", updateDeviceDto);

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var device = JsonSerializer.Deserialize<DeviceDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            Assert.NotNull(device);
            Assert.Equal("Updated Light", device.Name);
        }

        [Fact]
        public async Task DeleteDevice_WithValidId_ReturnsNoContent()
        {
            // Arrange
            await SeedTestDataAsync();

            // Act
            var response = await _client.DeleteAsync("/api/device/1");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        private async Task SeedTestDataAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            if (!await context.Users.AnyAsync())
            {
                var user = new User
                {
                    Id = 1,
                    Username = "testuser",
                    Email = "test@test.com",
                    FirstName = "Test",
                    LastName = "User",
                    PasswordHash = "hashedpassword",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var room = new Room
                {
                    Id = 1,
                    Name = "Test Room",
                    Description = "Test Description",
                    Floor = 1,
                    UserId = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var device = new Device
                {
                    Id = 1,
                    Name = "Test Device",
                    Type = "LIGHT",
                    Status = "OFF",
                    IsOnline = true,
                    RoomId = 1,
                    UserId = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(user);
                context.Rooms.Add(room);
                context.Devices.Add(device);
                await context.SaveChangesAsync();
            }
        }
    }

    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim("userId", "1"),
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.NameIdentifier, "1")
            };

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
} 