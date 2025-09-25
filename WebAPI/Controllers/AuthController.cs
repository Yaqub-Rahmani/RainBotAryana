using Business.Abstract;
using Entities.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    }
}
