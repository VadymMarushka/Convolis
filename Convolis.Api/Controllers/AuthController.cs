using Convolis.Api.Services.Abstractions;
using Convolis.Shared.DTOs;
using Convolis.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Convolis.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<TokenResponseDTO>> Register(UserDTO request)
        {
            var (user, error) = await authService.RegisterAsync(request);
            if (user is null)
                return BadRequest(error ?? "Registration failed.");

            var tokens = await authService.LoginAsync(request);
            return Ok(tokens);
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDTO>> Login(UserDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Username and password are required.");

            var result = await authService.LoginAsync(request);
            if (result is null)
                return BadRequest("Invalid username or password.");

            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDTO>> RefreshToken(RefreshTokenRequestDTO request)
        {
            var result = await authService.RefreshTokenAsync(request);
            if (result is null)
                return Unauthorized("Invalid or expired refresh token.");

            return Ok(result);
        }
    }
}