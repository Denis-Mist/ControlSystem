using ControlSystem.DTOs;
using ControlSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlSystem.Controllers
{
    [Authorize(Roles = "Manager")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly UserService _userService;

        public AdminController(UserService userService) => _userService = userService;

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users.Select(u => new { u.Id, u.Email, u.FullName }));
        }

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
        {
            var ok = await _userService.AssignRoleAsync(dto.UserId, dto.Role);
            if (!ok) return BadRequest(new { message = "Failed to assign role. Check role exists and user id." });
            return Ok(new { message = "Role assigned." });
        }

        [HttpPost("remove-role")]
        public async Task<IActionResult> RemoveRole([FromBody] AssignRoleDto dto)
        {
            var ok = await _userService.RemoveRoleAsync(dto.UserId, dto.Role);
            if (!ok) return BadRequest(new { message = "Failed to remove role." });
            return Ok(new { message = "Role removed." });
        }
    }
}
