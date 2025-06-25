using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using SmartHomeAutomation.API.DTOs;
using SmartHomeAutomation.API.Interfaces;
using SmartHomeAutomation.Core.Entities;
using SmartHomeAutomation.Core.Interfaces;

namespace SmartHomeAutomation.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AuthService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<UserDto> RegisterAsync(RegisterDto registerDto)
        {
            Console.WriteLine($"Register attempt - Username: {registerDto.Username}, Email: {registerDto.Email}");
            
            var userExists = await _unitOfWork.Users.FindAsync(u => u.Username == registerDto.Username);
            if (userExists.GetEnumerator().MoveNext())
            {
                Console.WriteLine("Username already exists");
                throw new Exception("Bu kullanıcı adı zaten kullanılıyor.");
            }

            var emailExists = await _unitOfWork.Users.FindAsync(u => u.Email == registerDto.Email);
            if (emailExists.GetEnumerator().MoveNext())
            {
                Console.WriteLine("Email already exists");
                throw new Exception("Bu e-posta adresi zaten kullanılıyor.");
            }

            var user = _mapper.Map<User>(registerDto);
            var passwordHash = HashPassword(registerDto.Password);
            user.PasswordHash = passwordHash;
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;
            
            Console.WriteLine($"Creating user - Username: {user.Username}, Email: {user.Email}");
            Console.WriteLine($"Password hash for registration: {passwordHash}");

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
            
            Console.WriteLine($"User created with ID: {user.Id}");

            return _mapper.Map<UserDto>(user);
        }

        public async Task<string> LoginAsync(LoginDto loginDto)
        {
            Console.WriteLine($"Login attempt - Username: {loginDto.Username}, Email: {loginDto.Email}");
            
            // Email veya username ile arama yap
            var loginIdentifier = !string.IsNullOrEmpty(loginDto.Email) ? loginDto.Email : loginDto.Username;
            Console.WriteLine($"Login identifier: {loginIdentifier}");
            
            var users = await _unitOfWork.Users.FindAsync(u => 
                u.Username == loginIdentifier || u.Email == loginIdentifier);
            var userList = users.ToList();
            
            Console.WriteLine($"Found {userList.Count} users");
            
            if (!userList.Any())
            {
                Console.WriteLine("No user found with given identifier");
                throw new Exception("Kullanıcı adı veya şifre hatalı.");
            }

            var user = userList.First();
            Console.WriteLine($"User found - ID: {user.Id}, Username: {user.Username}, IsActive: {user.IsActive}");
            
            var passwordHash = HashPassword(loginDto.Password);
            Console.WriteLine($"Password hash calculated: {passwordHash}");
            Console.WriteLine($"Stored password hash: {user.PasswordHash}");
            Console.WriteLine($"Hashes match: {user.PasswordHash == passwordHash}");
            
            if (user.PasswordHash != passwordHash)
            {
                Console.WriteLine("Password hash mismatch");
                throw new Exception("Kullanıcı adı veya şifre hatalı.");
            }

            if (!user.IsActive)
            {
                Console.WriteLine("User is not active");
                throw new Exception("Hesabınız aktif değil.");
            }

            Console.WriteLine("Login successful");
            // Gerçek uygulamada burada JWT token oluşturulup döndürülecek
            return "token";
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("Kullanıcı bulunamadı.");
            }

            if (user.PasswordHash != HashPassword(changePasswordDto.OldPassword))
            {
                throw new Exception("Mevcut şifre hatalı.");
            }

            if (changePasswordDto.NewPassword != changePasswordDto.ConfirmNewPassword)
            {
                throw new Exception("Yeni şifreler eşleşmiyor.");
            }

            user.PasswordHash = HashPassword(changePasswordDto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return true;
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
    }
} 