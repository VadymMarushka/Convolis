using Convolis.Api.Services.Abstractions;
using Convolis.Shared.DTOs;
using Convolis.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Convolis.Api.Controllers
{
    /// <summary>
    /// Handles HTTP requests related to user authentication, registration, and session management.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        /// <summary>
        /// Registers a new user and automatically logs them in.
        /// </summary>
        /// <param name="request">The user's chosen username and password.</param>
        /// <returns>A JWT access token and refresh token upon successful registration.</returns>
        [HttpPost("register")]
        public async Task<ActionResult<TokenResponseDTO>> Register(UserDTO request)
        {
            var (user, error) = await authService.RegisterAsync(request);
            if (user is null)
                return BadRequest(error ?? "Registration failed.");

            // Automatically authenticate the user right after successful registration
            var tokens = await authService.LoginAsync(request);
            return Ok(tokens);
        }

        /// <summary>
        /// Authenticates a user and provides session tokens.
        /// </summary>
        /// <param name="request">The user's credentials.</param>
        /// <returns>A JWT access token and refresh token.</returns>
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

        /// <summary>
        /// Refreshes an expired access token using a valid refresh token.
        /// </summary>
        /// <param name="request">The payload containing the user ID and refresh token.</param>
        /// <returns>A new set of access and refresh tokens.</returns>
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