using Convolis.Api.Data.Entities;
using Convolis.Shared.DTOs;
using Convolis.Shared.DTOs.Auth;

namespace Convolis.Api.Services.Abstractions
{
    /// <summary>
    /// Defines the contract for user authentication, registration, and JWT token management.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user and automatically assigns them to the Global Chat.
        /// </summary>
        /// <param name="request">The user registration details.</param>
        /// <returns>The created user entity and an error message if registration fails.</returns>
        Task<(User? user, string? error)> RegisterAsync(UserDTO request);
        /// <summary>
        /// Authenticates a user and generates access and refresh tokens.
        /// </summary>
        /// <param name="request">The user login credentials.</param>
        /// <returns>A token response containing the JWT and refresh token, or null if authentication fails.</returns>
        Task<TokenResponseDTO?> LoginAsync(UserDTO request);
        /// <summary>
        /// Generates a new access token using a valid refresh token.
        /// </summary>
        /// <param name="request">The refresh token request payload.</param>
        /// <returns>A new token response, or null if the refresh token is invalid or expired.</returns>
        Task<TokenResponseDTO?> RefreshTokenAsync(RefreshTokenRequestDTO request);
    }
}
