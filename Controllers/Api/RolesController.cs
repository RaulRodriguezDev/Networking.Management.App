using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Network.Management.App.Models.DTOs;
using Network.Management.App.Repository;
using Network.Management.App.Repository.Models;
using Network.Management.App.Services;

namespace Network.Management.App.Controllers.Api
{
    [Authorize(Policy = AppRoles.AdminOnly)]
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IDbRepository _repository;

        public RolesController(IDbRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _repository.GetAllAsync<Role, RoleDto>(r => new RoleDto
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                Description = r.Description
            });

            return Ok(roles);
        }
    }
}
