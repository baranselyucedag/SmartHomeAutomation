using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartHomeAutomation.API.DTOs;
using SmartHomeAutomation.API.Services;
using System.Security.Claims;

namespace SmartHomeAutomation.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly IWebHostEnvironment _environment;

        public RoomController(IRoomService roomService, IWebHostEnvironment environment)
        {
            _roomService = roomService;
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
        public async Task<ActionResult<List<RoomDto>>> GetAllRooms()
        {
            try 
            {
                var userId = GetUserId();
                Console.WriteLine($"UserId: {userId}");
                
                var rooms = await _roomService.GetAllRoomsAsync(userId);
                Console.WriteLine($"Rooms count: {rooms?.Count() ?? 0}");
                
                return Ok(rooms);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllRooms: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RoomDto>> GetRoom(int id)
        {
            try
            {
                var userId = GetUserId();
                var room = await _roomService.GetRoomByIdAsync(id, userId);
                return Ok(room);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<ActionResult<RoomDto>> CreateRoom([FromBody] CreateRoomDto createRoomDto)
        {
            var userId = GetUserId();
            var room = await _roomService.CreateRoomAsync(createRoomDto, userId);
            return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, room);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<RoomDto>> UpdateRoom(int id, [FromBody] UpdateRoomDto updateRoomDto)
        {
            try
            {
                var userId = GetUserId();
                var room = await _roomService.UpdateRoomAsync(id, updateRoomDto, userId);
                return Ok(room);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            try
            {
                var userId = GetUserId();
                await _roomService.DeleteRoomAsync(id, userId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("{id}/devices")]
        public async Task<ActionResult<List<DeviceDto>>> GetRoomDevices(int id)
        {
            try
            {
                var userId = GetUserId();
                var room = await _roomService.GetRoomByIdAsync(id, userId);
                return Ok(room.Devices);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
} 