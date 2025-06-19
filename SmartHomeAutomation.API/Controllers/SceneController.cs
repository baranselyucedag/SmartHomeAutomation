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
    public class SceneController : ControllerBase
    {
        private readonly ISceneService _sceneService;
        private readonly IWebHostEnvironment _environment;

        public SceneController(ISceneService sceneService, IWebHostEnvironment environment)
        {
            _sceneService = sceneService;
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
        public async Task<ActionResult<List<SceneDto>>> GetAllScenes()
        {
            var userId = GetUserId();
            var scenes = await _sceneService.GetAllScenesAsync(userId);
            return Ok(scenes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SceneDto>> GetScene(int id)
        {
            try
            {
                var userId = GetUserId();
                var scene = await _sceneService.GetSceneByIdAsync(id, userId);
                return Ok(scene);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<ActionResult<SceneDto>> CreateScene([FromBody] CreateSceneDto createSceneDto)
        {
            var userId = GetUserId();
            var scene = await _sceneService.CreateSceneAsync(createSceneDto, userId);
            return CreatedAtAction(nameof(GetScene), new { id = scene.Id }, scene);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<SceneDto>> UpdateScene(int id, [FromBody] UpdateSceneDto updateSceneDto)
        {
            try
            {
                var userId = GetUserId();
                var scene = await _sceneService.UpdateSceneAsync(id, updateSceneDto, userId);
                return Ok(scene);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteScene(int id)
        {
            try
            {
                var userId = GetUserId();
                await _sceneService.DeleteSceneAsync(id, userId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("{id}/execute")]
        public async Task<IActionResult> ExecuteScene(int id)
        {
            try
            {
                var userId = GetUserId();
                await _sceneService.ExecuteSceneAsync(id, userId);
                return Ok(new { message = "Scene executed successfully" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{id}/devices")]
        public async Task<ActionResult<List<DeviceDto>>> GetSceneDevices(int id)
        {
            try
            {
                var userId = GetUserId();
                var devices = await _sceneService.GetSceneDevicesAsync(id, userId);
                return Ok(devices);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("{id}/schedule")]
        public async Task<IActionResult> ScheduleScene(int id, [FromBody] SceneScheduleDto scheduleDto)
        {
            try
            {
                var userId = GetUserId();
                await _sceneService.ScheduleSceneAsync(id, scheduleDto, userId);
                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (NotImplementedException)
            {
                return StatusCode(501, "Scene scheduling is not implemented yet");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while scheduling the scene: {ex.Message}");
            }
        }
    }
} 