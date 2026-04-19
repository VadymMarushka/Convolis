using Convolis.Shared.DTOs;
using Convolis.Shared.DTOs.Auth;
using System.Net.Http.Json;

namespace Convolis.UI.Services
{
    /// <summary>
    /// Handles user authentication, registration, and token management via the backend API.
    /// </summary>
    public class AuthService(HttpClient http, AuthStateService authState)
    {
        public async Task<(bool success, string? error)> LoginAsync(string username, string password)
        {
            try
            {
                var response = await http.PostAsJsonAsync("api/auth/login", new UserDTO
                {
                    Username = username,
                    Password = password
                });

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return (false, error.Trim('"'));
                }

                var tokens = await response.Content.ReadFromJsonAsync<TokenResponseDTO>();
                if (tokens != null)
                    await authState.SetTokensAsync(tokens);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool success, string? error)> RegisterAsync(string username, string password)
        {
            try
            {
                var response = await http.PostAsJsonAsync("api/auth/register", new UserDTO
                {
                    Username = username,
                    Password = password
                });

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return (false, error.Trim('"'));
                }

                var tokens = await response.Content.ReadFromJsonAsync<TokenResponseDTO>();
                if (tokens != null)
                    await authState.SetTokensAsync(tokens);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Attempts to silently refresh the JWT access token using the stored refresh token.
        /// </summary>
        public async Task<bool> TryRefreshAsync()
        {
            if (authState.UserId == null || string.IsNullOrEmpty(authState.RefreshToken))
                return false;

            try
            {
                var response = await http.PostAsJsonAsync("api/auth/refresh-token", new RefreshTokenRequestDTO
                {
                    Id = authState.UserId.Value,
                    RefreshToken = authState.RefreshToken
                });

                if (!response.IsSuccessStatusCode) return false;

                var tokens = await response.Content.ReadFromJsonAsync<TokenResponseDTO>();
                if (tokens != null)
                    await authState.SetTokensAsync(tokens);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}