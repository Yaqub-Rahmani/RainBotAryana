using Business.Abstract;
using Entities.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody] UserForRegisterDto dto)
        {
            var exists = _authService.UserExists(dto.Email);
            if (!exists.Success) return BadRequest(exists.Message);

            var registerResult = _authService.Register(dto, dto.Password);
            if (!registerResult.Success) return BadRequest(registerResult.Message);

            var tokenResult = _authService.CreateAccessToken(registerResult.Data);
            return tokenResult.Success ? Ok(tokenResult.Data) : BadRequest(tokenResult.Message);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserForLoginDto dto)
        {
            var userToLogin = _authService.Login(dto);
            if (!userToLogin.Success) return BadRequest(userToLogin.Message);

            var tokenResult = _authService.CreateAccessToken(userToLogin.Data);
            return tokenResult.Success ? Ok(tokenResult.Data) : BadRequest(tokenResult.Message);
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var user = HttpContext.User;

            var id = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                     ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var email = user.FindFirst(ClaimTypes.Email)?.Value;
            var fullName = user.FindFirst(ClaimTypes.Name)?.Value;

            string? firstName = null, lastName = null;
            if (!string.IsNullOrWhiteSpace(fullName))
            {
                var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                firstName = parts.FirstOrDefault();
                lastName = parts.Length > 1 ? string.Join(' ', parts.Skip(1)) : null;
            }

            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            return Ok(new
            {
                id,
                email,
                firstName,
                lastName,
                roles
            });
        }
    }
}
