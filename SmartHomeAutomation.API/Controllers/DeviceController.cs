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
    [Authorize]
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
            // JWT claim'den userId al
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            
            if (string.IsNullOrEmpty(userIdClaim))
            {
                Console.WriteLine("User ID not found in claims. Available claims:");
                foreach (var claim in User.Claims)
                {
                    Console.WriteLine($"  {claim.Type}: {claim.Value}");
                }
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            
            Console.WriteLine($"GetUserId called - returning user ID: {userIdClaim}");
            return int.Parse(userIdClaim);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeviceDto>>> GetAllDevices()
        {
            try
            {
                var userId = GetUserId();
                // Get only devices that belong to the current user
                var devices = await _deviceService.GetAllDevicesAsync(userId);
            return Ok(devices);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllDevices: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DeviceDto>> GetDevice(int id)
        {
            try
            {
                var userId = GetUserId();
                var device = await _deviceService.GetDeviceByIdAsync(id, userId);
                return Ok(device);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid("Bu cihaza erişim yetkiniz yok.");
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<ActionResult<DeviceDto>> CreateDevice([FromBody] CreateDeviceDto createDeviceDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            // Set the user ID on the DTO
            createDeviceDto.UserId = userId;
            
            var device = await _deviceService.CreateDeviceAsync(createDeviceDto);
            return CreatedAtAction(nameof(GetDevice), new { id = device.Id }, device);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DeviceDto>> UpdateDevice(int id, [FromBody] UpdateDeviceDto updateDeviceDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GetUserId();
                var device = await _deviceService.UpdateDeviceAsync(id, updateDeviceDto, userId);
                return Ok(device);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid("Bu cihaza erişim yetkiniz yok.");
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
                var userId = GetUserId();
                var success = await _deviceService.DeleteDeviceAsync(id, userId);
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
                var userId = GetUserId();
                var status = await _deviceService.GetDeviceStatusAsync(id, userId);
                return Ok(status);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid("Bu cihaza erişim yetkiniz yok.");
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("{id}/status")]
        public async Task<ActionResult<DeviceStatusDto>> UpdateDeviceStatus(int id, [FromBody] DeviceStatusDto statusDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GetUserId();
                var status = await _deviceService.UpdateDeviceStatusAsync(id, statusDto, userId);
                return Ok(status);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid("Bu cihaza erişim yetkiniz yok.");
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
                var userId = GetUserId();
                var currentStatus = await _deviceService.GetDeviceStatusAsync(id, userId);
                // Toggle the online status since there's no IsActive property
                currentStatus.Status = currentStatus.Status == "ON" ? "OFF" : "ON";
                
                var updatedStatus = await _deviceService.UpdateDeviceStatusAsync(id, currentStatus, userId);
                return Ok(updatedStatus);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid("Bu cihaza erişim yetkiniz yok.");
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