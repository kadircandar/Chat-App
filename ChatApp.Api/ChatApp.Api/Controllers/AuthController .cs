using ChatApp.Api.DTOs.Auth;
using ChatApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
namespace ChatApp.Api.Controllers

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

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _authService.Register(dto);

            if (result.Success) { return Ok(); }

            return BadRequest(result.Message);
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.Login(dto);

            if (result.Success) { return Ok(result.Data); }

            return BadRequest(result.Message);
        }
    }
}
