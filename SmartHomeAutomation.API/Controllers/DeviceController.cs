using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SmartHomeAutomation.API.DTOs;
using SmartHomeAutomation.API.Interfaces;
using System.Security.Claims;

namespace SmartHomeAutomation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("DevelopmentPolicy")]
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceService _deviceService;
        private readonly IWebHostEnvironment _environment;

        public DeviceController(IDeviceService deviceService, IWebHostEnvironment environment)
        {
            _deviceService = deviceService;
            _environment = environment;
        }

        private int GetUserId()
        {
            // For development, use a default user ID
            if (_environment.IsDevelopment())
            {
                return 1; // Default user ID for development
            }
            
            // For production, get the user ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User ID not found in claims");
            }
            
            return int.Parse(userIdClaim);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeviceDto>>> GetAllDevices()
        {
            // For development purposes, get all devices
            var devices = await _deviceService.GetAllDevicesAsync();
            return Ok(devices);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DeviceDto>> GetDevice(int id)
        {
            try
            {
                var device = await _deviceService.GetDeviceByIdAsync(id);
                return Ok(device);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<ActionResult<DeviceDto>> CreateDevice([FromBody] CreateDeviceDto createDeviceDto)
        {
            var userId = GetUserId();
            // Set the user ID on the DTO
            createDeviceDto.UserId = userId;
            
            var device = await _deviceService.CreateDeviceAsync(createDeviceDto);
            return CreatedAtAction(nameof(GetDevice), new { id = device.Id }, device);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DeviceDto>> UpdateDevice(int id, [FromBody] UpdateDeviceDto updateDeviceDto)
        {
            try
            {
                var device = await _deviceService.UpdateDeviceAsync(id, updateDeviceDto);
                return Ok(device);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDevice(int id)
        {
            try
            {
                var success = await _deviceService.DeleteDeviceAsync(id);
                if (success)
                {
                    return NoContent();
                }
                return NotFound();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the device");
            }
        }

        [HttpGet("{id}/status")]
        public async Task<ActionResult<DeviceStatusDto>> GetDeviceStatus(int id)
        {
            try
            {
                var status = await _deviceService.GetDeviceStatusAsync(id);
                return Ok(status);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("{id}/status")]
        public async Task<ActionResult<DeviceStatusDto>> UpdateDeviceStatus(int id, [FromBody] DeviceStatusDto statusDto)
        {
            try
            {
                var status = await _deviceService.UpdateDeviceStatusAsync(id, statusDto);
                return Ok(status);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("{id}/toggle")]
        public async Task<ActionResult> ToggleDevice(int id)
        {
            try
            {
                var currentStatus = await _deviceService.GetDeviceStatusAsync(id);
                // Toggle the online status since there's no IsActive property
                currentStatus.Status = currentStatus.Status == "ON" ? "OFF" : "ON";
                
                var updatedStatus = await _deviceService.UpdateDeviceStatusAsync(id, currentStatus);
                return Ok(updatedStatus);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while toggling the device: {ex.Message}");
            }
        }
    }
} 