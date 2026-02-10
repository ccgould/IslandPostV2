using IslandPostApi.Contracts;
using IslandPostPOS.Shared.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IslandPostApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccessController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _config;

        public AccessController(IUserService userService, IConfiguration config)
        {
            _userService = userService;
            _config = config;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO model)
        {
            try
            {
                // Validate credentials
                var user_found = await _userService.GetByCredentials(model.Email, model.PassWord);

                if (user_found == null)
                {
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                // Build claims
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user_found.Name ?? ""),
            new Claim(ClaimTypes.NameIdentifier, user_found.IdUsers.ToString()),
            new Claim(ClaimTypes.Role, user_found.IdRol?.ToString() ?? ""),
            new Claim("Email", user_found.Email ?? "")
        };

                // Generate JWT token
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(2),
                    signingCredentials: creds);

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                // Return token + user info
                return Ok(new LoginResponseDTO()
                {
                    Message = "Login successful",
                    Token = tokenString,
                    User = new UserDTO()
                    {
                       IdUsers = user_found.IdUsers,
                       Name = user_found.Name,
                       Email = user_found.Email,
                       IdRol = user_found.IdRol
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }
    }
}
