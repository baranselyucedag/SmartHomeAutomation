using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartHomeAutomation.API.DTOs;
using SmartHomeAutomation.API.Interfaces;
using SmartHomeAutomation.Core.Entities;
using SmartHomeAutomation.Core.Interfaces;
using System.Security.Claims;
using SmartHomeAutomation.API.Services;

namespace SmartHomeAutomation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("DevelopmentPolicy")]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly IAuthService _authService;

        public UserController(IUnitOfWork unitOfWork, IJwtService jwtService, IAuthService authService)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _authService = authService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            
            return int.Parse(userIdClaim);
        }

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
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                // AuthService'i kullan - ortak hash algoritması için
                var token = await _authService.LoginAsync(loginDto);
                
                // Kullanıcı bilgilerini al
                var loginIdentifier = !string.IsNullOrEmpty(loginDto.Email) ? loginDto.Email : loginDto.Username;
                var users = await _unitOfWork.Users.FindAsync(u => 
                    u.Username == loginIdentifier || u.Email == loginIdentifier);
                var user = users.First();

                // JWT token oluştur
                var jwtToken = _jwtService.GenerateToken(user.Id, user.Username, user.Email);

                return Ok(new
                {
                    message = "Giriş başarılı",
                    token = jwtToken,
                    user = new
                    {
                        id = user.Id,
                        username = user.Username,
                        email = user.Email,
                        firstName = user.FirstName,
                        lastName = user.LastName
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var userExists = await _unitOfWork.Users.FindAsync(u => u.Username == registerDto.Username);
                if (userExists.Any())
                {
                    return BadRequest(new { message = "Bu kullanıcı adı zaten kullanılıyor." });
                }

                var emailExists = await _unitOfWork.Users.FindAsync(u => u.Email == registerDto.Email);
                if (emailExists.Any())
                {
                    return BadRequest(new { message = "Bu e-posta adresi zaten kullanılıyor." });
                }

                // AuthService'i kullan - ortak hash algoritması için
                var userDto = await _authService.RegisterAsync(registerDto);
                
                // Oluşturulan kullanıcıyı al (AuthService zaten kaydetti)
                var users = await _unitOfWork.Users.FindAsync(u => u.Username == registerDto.Username);
                var user = users.First();

                return Ok(new
                {
                    message = "Kayıt başarılı",
                    user = new
                    {
                        id = user.Id,
                        username = user.Username,
                        email = user.Email,
                        firstName = user.FirstName,
                        lastName = user.LastName
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası: " + ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // JWT stateless olduğu için backend'de logout işlemi yok
                // Frontend'de token'ı temizlemek yeterli
                return Ok(new { message = "Çıkış başarılı" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Çıkış yapılırken hata: " + ex.Message });
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                Console.WriteLine($"Change password request received");
                Console.WriteLine($"Request body: OldPassword={changePasswordDto?.OldPassword?.Length} chars, NewPassword={changePasswordDto?.NewPassword?.Length} chars");
                
                var userId = GetUserId();
                Console.WriteLine($"User ID from token: {userId}");
                
                var result = await _authService.ChangePasswordAsync(userId, changePasswordDto);
                
                if (result)
                {
                    Console.WriteLine("Password change successful");
                    return Ok(new { message = "Şifre başarıyla değiştirildi" });
                }
                
                Console.WriteLine("Password change failed");
                return BadRequest(new { message = "Şifre değiştirilemedi" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Change password error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("dev/recreate-test-user")]
        public async Task<IActionResult> RecreateTestUser()
        {
            try
            {
                // Delete existing test user
                var existingUsers = await _unitOfWork.Users.FindAsync(u => u.Username == "testuser");
                foreach (var existingUser in existingUsers)
                {
                    await _unitOfWork.Users.DeleteAsync(existingUser.Id);
                }
                await _unitOfWork.SaveChangesAsync();

                // Create new test user
                // AuthService ile register et - ortak hash algoritması için
                var registerDto = new RegisterDto
                {
                    Username = "testuser",
                    Email = "test@example.com",
                    FirstName = "Test",
                    LastName = "User",
                    Password = "123456"
                };
                
                var userDto = await _authService.RegisterAsync(registerDto);
                
                // Oluşturulan kullanıcıyı al (AuthService zaten kaydetti)
                var users = await _unitOfWork.Users.FindAsync(u => u.Username == "testuser");
                var testUser = users.First();

                return Ok(new
                {
                    message = "Test kullanıcısı oluşturuldu",
                    credentials = new
                    {
                        username = "testuser",
                        password = "123456"
                    },
                    user = new
                    {
                        id = testUser.Id,
                        username = testUser.Username,
                        email = testUser.Email
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Test kullanıcısı oluşturulurken hata: " + ex.Message });
            }
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