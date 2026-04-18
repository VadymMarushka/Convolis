using Convolis.Shared.DTOs;
using Convolis.Shared.DTOs.Auth;
using System.Net.Http.Json;

namespace Convolis.UI.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly AuthStateService _authState;

        public AuthService(HttpClient http, AuthStateService authState)
        {
            _http = http;
            _authState = authState;
        }

        public async Task<(bool success, string? error)> LoginAsync(string username, string password)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/login", new UserDTO
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
                    await _authState.SetTokensAsync(tokens);

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
                var response = await _http.PostAsJsonAsync("api/auth/register", new UserDTO
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
                    await _authState.SetTokensAsync(tokens);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<bool> TryRefreshAsync()
        {
            if (_authState.UserId == null || string.IsNullOrEmpty(_authState.RefreshToken))
                return false;

            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/refresh-token", new RefreshTokenRequestDTO
                {
                    Id = _authState.UserId.Value,
                    RefreshToken = _authState.RefreshToken
                });

                if (!response.IsSuccessStatusCode) return false;

                var tokens = await response.Content.ReadFromJsonAsync<TokenResponseDTO>();
                if (tokens != null)
                    await _authState.SetTokensAsync(tokens);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
