using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SmartHomeAutomation.API.DTOs;
using SmartHomeAutomation.API.Interfaces;
using SmartHomeAutomation.Core.Entities;
using SmartHomeAutomation.Core.Interfaces;
using System.Security.Cryptography;
using System.Text;
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

        public UserController(IUnitOfWork unitOfWork, IJwtService jwtService)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
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
                // Email veya username ile arama yap
                var loginIdentifier = !string.IsNullOrEmpty(loginDto.Email) ? loginDto.Email : loginDto.Username;
                var users = await _unitOfWork.Users.FindAsync(u => 
                    u.Username == loginIdentifier || u.Email == loginIdentifier);
                var userList = users.ToList();
                
                if (!userList.Any())
                {
                    return BadRequest(new { message = "Kullanıcı adı veya şifre hatalı." });
                }

                var user = userList.First();
                var passwordHash = HashPassword(loginDto.Password);
                
                if (user.PasswordHash != passwordHash)
                {
                    return BadRequest(new { message = "Kullanıcı adı veya şifre hatalı." });
                }

                if (!user.IsActive)
                {
                    return BadRequest(new { message = "Hesabınız aktif değil." });
                }

                // Generate JWT token
                var token = _jwtService.GenerateToken(user.Id, user.Username, user.Email);

                return Ok(new
                {
                    message = "Giriş başarılı",
                    token = token,
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

                var user = new User
                {
                    Username = registerDto.Username,
                    Email = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    PasswordHash = HashPassword(registerDto.Password),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

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
                var testUser = new User
                {
                    Username = "testuser",
                    Email = "test@example.com",
                    FirstName = "Test",
                    LastName = "User",
                    PasswordHash = HashPassword("123456"),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _unitOfWork.Users.AddAsync(testUser);
                await _unitOfWork.SaveChangesAsync();

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

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
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