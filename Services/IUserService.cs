using Network.Management.App.Models.DTOs;
using Network.Management.App.Repository.Models;

namespace Network.Management.App.Services
{
    public interface IUserService
    {
        /// <summary>Validates credentials and returns the matching User, or null if invalid.</summary>
        Task<User?> ValidateAsync(string username, string password);

        Task<IEnumerable<UserDto>> GetAllAsync();

        Task<UserDto?> GetByIdAsync(int userId);

        Task<bool> UsernameExistsAsync(string username);

        Task<UserDto> CreateAsync(CreateUserDto dto, int requestingUserId);

        Task<UserDto?> UpdateAsync(int userId, UpdateUserDto dto, int requestingUserId);

        Task<bool> DeleteAsync(int userId, int requestingUserId);
    }
}
