using Microsoft.EntityFrameworkCore;
using Network.Management.App.Models.DTOs;
using Network.Management.App.Repository;
using Network.Management.App.Repository.Models;

namespace Network.Management.App.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IAppLogger _logger;

        public UserService(AppDbContext context, IAppLogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User?> ValidateAsync(string username, string password)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username.ToLowerInvariant());

            if (user is null || !PasswordHasher.Verify(password, user.Password))
                return null;

            return user;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            return await _context.Users
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    Username = u.Username,
                    RoleName = u.Role.RoleName
                })
                .ToListAsync();
        }

        public async Task<UserDto?> GetByIdAsync(int userId)
        {
            return await _context.Users
                .Where(u => u.UserId == userId)
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    Username = u.Username,
                    RoleName = u.Role.RoleName
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username.ToLowerInvariant());
        }

        public async Task<UserDto> CreateAsync(CreateUserDto dto, int requestingUserId)
        {
            var user = new User
            {
                Name = dto.Name,
                Username = dto.Username.ToLowerInvariant(),
                Password = PasswordHasher.Hash(dto.Password),
                RoleId = dto.RoleId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _logger.PersistAsync(LogActionType.UserCreated, $"User '{user.Username}' created.", requestingUserId);

            return new UserDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Username = user.Username,
                RoleName = (await _context.Roles.FindAsync(user.RoleId))?.RoleName ?? string.Empty
            };
        }

        public async Task<UserDto?> UpdateAsync(int userId, UpdateUserDto dto, int requestingUserId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user is null) return null;

            user.Name = dto.Name;
            user.RoleId = dto.RoleId;

            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.Password = PasswordHasher.Hash(dto.Password);

            await _context.SaveChangesAsync();

            await _logger.PersistAsync(LogActionType.UserUpdated, $"User '{user.Username}' updated.", requestingUserId);

            return await GetByIdAsync(userId);
        }

        public async Task<bool> DeleteAsync(int userId, int requestingUserId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user is null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            await _logger.PersistAsync(LogActionType.UserDeleted, $"User '{user.Username}' deleted.", requestingUserId);

            return true;
        }
    }
}
