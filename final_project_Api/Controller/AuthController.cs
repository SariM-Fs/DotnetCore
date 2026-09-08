using final_project_API.DTOs;
using final_project_Core.Common;
using final_project_Core.Interface;
using Microsoft.AspNetCore.Mvc;

namespace final_project_API.Controllers
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

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request, CancellationToken ct)
        {
            var result = await _authService.RegisterAsync(request.Name, request.Email, request.Password, request.LicensePlate, ct);

            if (result.Status == OperationStatus.Conflict)
            {
                return Conflict(new { message = result.Message });
            }

            var (driver, token) = result.Data;
            return Ok(new AuthResponseDto { Token = token, Name = driver.Name, Email = driver.Email, Role = driver.Role });
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken ct)
        {
            var result = await _authService.LoginAsync(request.Email, request.Password, ct);

            if (result.Status == OperationStatus.ValidationError)
            {
                return Unauthorized(new { message = result.Message });
            }

            var (driver, token) = result.Data;
            return Ok(new AuthResponseDto { Token = token, Name = driver.Name, Email = driver.Email, Role = driver.Role });
        }
    }
}
