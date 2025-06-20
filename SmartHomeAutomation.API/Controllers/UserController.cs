using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace SmartHomeAutomation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("DevelopmentPolicy")]
    public class UserController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            return Ok("Get all users");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            return Ok($"Get user with id: {id}");
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] object userDto)
        {
            return CreatedAtAction(nameof(GetUser), new { id = 1 }, userDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] object userDto)
        {
            return Ok($"Update user with id: {id}");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            return NoContent();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] object loginDto)
        {
            return Ok("User logged in successfully");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] object registerDto)
        {
            return Ok("User registered successfully");
        }

        [HttpGet("{id}/preferences")]
        public async Task<IActionResult> GetUserPreferences(int id)
        {
            return Ok($"Get preferences for user with id: {id}");
        }

        [HttpPut("{id}/preferences")]
        public async Task<IActionResult> UpdateUserPreferences(int id, [FromBody] object preferencesDto)
        {
            return Ok($"Update preferences for user with id: {id}");
        }
    }
} 