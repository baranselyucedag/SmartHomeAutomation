using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SmartHomeAutomation.API.DTOs;
using SmartHomeAutomation.API.Services;
using System.Security.Claims;

namespace SmartHomeAutomation.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [EnableCors("DevelopmentPolicy")]
    [Authorize]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<RoomController> _logger;

        public RoomController(IRoomService roomService, IWebHostEnvironment environment, ILogger<RoomController> logger)
        {
            _roomService = roomService;
            _environment = environment;
            _logger = logger;
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetRoom: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<RoomDto>> CreateRoom([FromBody] CreateRoomDto createRoomDto)
        {
            try
            {
                Console.WriteLine($"CreateRoom called with: {System.Text.Json.JsonSerializer.Serialize(createRoomDto)}");
                
                if (createRoomDto == null)
                {
                    Console.WriteLine("CreateRoomDto is null");
                    return BadRequest(new { message = "Room data is required" });
                }

                var userId = GetUserId();
                Console.WriteLine($"UserId: {userId}");
                
                // UserId will be set in service layer
                
                var room = await _roomService.CreateRoomAsync(createRoomDto, userId);
                Console.WriteLine($"Room created with ID: {room.Id}");
                
                return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, room);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateRoom: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = ex.Message, detail = ex.StackTrace });
            }
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateRoom: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = ex.Message });
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteRoom: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = ex.Message });
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetRoomDevices: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("paginated")]
        public async Task<ActionResult<PaginationDto<RoomDto>>> GetPaginatedRooms([FromQuery] PaginationParams paginationParams)
        {
            try
            {
                var userId = GetUserId();
                var paginatedRooms = await _roomService.GetPaginatedRoomsAsync(userId, paginationParams);
                return Ok(paginatedRooms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated rooms");
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
} 