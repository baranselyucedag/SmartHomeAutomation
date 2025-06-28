using System.Threading.Tasks;
using SmartHomeAutomation.API.DTOs;

namespace SmartHomeAutomation.API.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetUserByIdAsync(int id);
        Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
        Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(int id);
        Task<UserDto> AuthenticateAsync(string username, string password);
    }
} 