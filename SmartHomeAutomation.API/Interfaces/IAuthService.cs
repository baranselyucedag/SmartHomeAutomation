using System.Threading.Tasks;

namespace SmartHomeAutomation.API.Interfaces
{
    public interface IAuthService
    {
        Task<DTOs.UserDto> RegisterAsync(DTOs.RegisterDto registerDto);
        Task<string> LoginAsync(DTOs.LoginDto loginDto);
        Task<bool> ChangePasswordAsync(int userId, DTOs.ChangePasswordDto changePasswordDto);
    }
} 