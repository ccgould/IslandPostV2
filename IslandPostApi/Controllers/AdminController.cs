using IslandPostApi.Contracts;
using IslandPostApi.Models;
using IslandPostPOS.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IslandPostApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;

        public AdminController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            var users = await _userService.List();

            var dtos = users.Select(u => new UserDTO
            {
                IdUsers = u.IdUsers,
                Name = u.Name,
                Email = u.Email,
                Phone = u.Phone,
                IdRol = u.IdRol,
                IsActive = u.IsActive,
                RegistrationDate = u.RegistrationDate,
                RoleName = u.IdRolNavigation?.Description
            }).ToList();

            return Ok(dtos);
        }

        [HttpPost("users")]
        public async Task<ActionResult<UserDTO>> CreateUser([FromBody] UserDTO dto)
        {
            try
            {
                var entity = new User
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    Phone = dto.Phone,
                    IdRol = dto.IdRol,
                    IsActive = dto.IsActive,
                    RegistrationDate = dto.RegistrationDate ?? DateTime.UtcNow
                };

                var created = await _userService.Add(entity);

                var createdDto = new UserDTO
                {
                    IdUsers = created.IdUsers,
                    Name = created.Name,
                    Email = created.Email,
                    Phone = created.Phone,
                    IdRol = created.IdRol,
                    IsActive = created.IsActive,
                    RegistrationDate = created.RegistrationDate,
                    RoleName = created.IdRolNavigation?.Description
                };

                return Ok(createdDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //[HttpPut("users/{id:int}")]
        //public async Task<ActionResult<UserDTO>> EditUser(int id, [FromBody] UserDTO dto)
        //{
        //    try
        //    {
        //        var user = await _userService.GetById(id);
        //        if (user == null)
        //            return NotFound("User not found.");

        //        // Update fields
        //        user.Name = dto.Name ?? user.Name;
        //        user.Email = dto.Email ?? user.Email;
        //        user.Phone = dto.Phone ?? user.Phone;
        //        user.IdRol = dto.IdRol ?? user.IdRol;
        //        user.IsActive = dto.IsActive ?? user.IsActive;

        //        await _userService.Update(user);

        //        var updatedDto = new UserDTO
        //        {
        //            IdUsers = user.IdUsers,
        //            Name = user.Name,
        //            Email = user.Email,
        //            Phone = user.Phone,
        //            IdRol = user.IdRol,
        //            IsActive = user.IsActive,
        //            RegistrationDate = user.RegistrationDate,
        //            RoleName = user.IdRolNavigation?.Description
        //        };

        //        return Ok(updatedDto);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}

        [HttpDelete("users/{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _userService.GetById(id);
                if (user == null)
                    return NotFound("User not found.");

                await _userService.Delete(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}