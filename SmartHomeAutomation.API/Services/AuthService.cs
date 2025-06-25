using System;
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
            var userExists = await _unitOfWork.Users.FindAsync(u => u.Username == registerDto.Username);
            if (userExists.GetEnumerator().MoveNext())
            {
                throw new Exception("Bu kullanıcı adı zaten kullanılıyor.");
            }

            var emailExists = await _unitOfWork.Users.FindAsync(u => u.Email == registerDto.Email);
            if (emailExists.GetEnumerator().MoveNext())
            {
                throw new Exception("Bu e-posta adresi zaten kullanılıyor.");
            }

            var user = _mapper.Map<User>(registerDto);
            user.PasswordHash = HashPassword(registerDto.Password);
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task<string> LoginAsync(LoginDto loginDto)
        {
            // Email veya username ile arama yap
            var loginIdentifier = !string.IsNullOrEmpty(loginDto.Email) ? loginDto.Email : loginDto.Username;
            var users = await _unitOfWork.Users.FindAsync(u => 
                u.Username == loginIdentifier || u.Email == loginIdentifier);
            var userList = users.ToList();
            
            if (!userList.Any())
            {
                throw new Exception("Kullanıcı adı veya şifre hatalı.");
            }

            var user = userList.First();
            var passwordHash = HashPassword(loginDto.Password);
            
            if (user.PasswordHash != passwordHash)
            {
                throw new Exception("Kullanıcı adı veya şifre hatalı.");
            }

            if (!user.IsActive)
            {
                throw new Exception("Hesabınız aktif değil.");
            }

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