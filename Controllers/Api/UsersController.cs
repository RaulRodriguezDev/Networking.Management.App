using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Network.Management.App.Models.DTOs;
using Network.Management.App.Services;
using System.Security.Claims;

namespace Network.Management.App.Controllers.Api
{
    [Authorize(Policy = AppRoles.AdminOnly)]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _userService.GetAllAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            return user is null ? NotFound() : Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            if (await _userService.UsernameExistsAsync(dto.Username))
                return Conflict(new { message = $"Username '{dto.Username}' is already taken." });

            var created = await _userService.CreateAsync(dto, RequestingUserId());
            return CreatedAtAction(nameof(GetById), new { id = created.UserId }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
        {
            var updated = await _userService.UpdateAsync(id, dto, RequestingUserId());
            return updated is null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _userService.DeleteAsync(id, RequestingUserId());
            return deleted ? NoContent() : NotFound();
        }

        private int RequestingUserId() =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    }
}
