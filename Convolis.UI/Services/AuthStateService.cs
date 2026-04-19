using Convolis.Shared.DTOs;
using Convolis.Shared.DTOs.Auth;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Convolis.UI.Services
{
    /// <summary>
    /// Manages the client-side authentication state, including JWT storage, parsing, and expiration tracking.
    /// Triggers UI updates when the authentication state changes.
    /// </summary>
    public class AuthStateService(IJSRuntime js)
    {
        public string? AccessToken { get; private set; }
        public string? RefreshToken { get; private set; }
        public Guid? UserId { get; private set; }
        public string? Username { get; private set; }

        public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);

        /// <summary>
        /// Checks if the access token is expired or about to expire within the next 30 seconds.
        /// The 30-second buffer prevents tokens from expiring mid-flight during an HTTP request.
        /// </summary>
        public bool IsAccessTokenExpired()
        {
            if (string.IsNullOrEmpty(AccessToken)) return true;
            try
            {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(AccessToken);
                return jwt.ValidTo < DateTime.UtcNow.AddSeconds(30);
            }
            catch { return true; }
        }

        // Event triggered whenever the user logs in or logs out, allowing Blazor components to re-render
        public event Action? OnChange;

        /// <summary>
        /// Restores the authentication session from the browser's local storage upon application startup.
        /// </summary>
        public async Task InitializeAsync()
        {
            AccessToken = await js.InvokeAsync<string?>("localStorage.getItem", "accessToken");
            RefreshToken = await js.InvokeAsync<string?>("localStorage.getItem", "refreshToken");

            if (!string.IsNullOrEmpty(AccessToken))
                ParseToken(AccessToken);
        }

        /// <summary>
        /// Saves the new authentication tokens to local storage and updates the in-memory state.
        /// </summary>
        public async Task SetTokensAsync(TokenResponseDTO tokens)
        {
            AccessToken = tokens.AccessToken;
            RefreshToken = tokens.RefreshToken;
            ParseToken(tokens.AccessToken);

            await js.InvokeVoidAsync("localStorage.setItem", "accessToken", tokens.AccessToken);
            await js.InvokeVoidAsync("localStorage.setItem", "refreshToken", tokens.RefreshToken);

            OnChange?.Invoke();
        }

        /// <summary>
        /// Clears the authentication session, removing tokens from memory and local storage.
        /// </summary>
        public async Task ClearAsync()
        {
            AccessToken = null;
            RefreshToken = null;
            UserId = null;
            Username = null;

            await js.InvokeVoidAsync("localStorage.removeItem", "accessToken");
            await js.InvokeVoidAsync("localStorage.removeItem", "refreshToken");

            OnChange?.Invoke();
        }

        /// <summary>
        /// Extracts user identity information (Username and UserId) directly from the JWT payload 
        /// without requiring an additional trip to the backend.
        /// </summary>
        private void ParseToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            // Handles both standard claim types and standard JWT named claims
            Username = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                    ?? jwt.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value;

            var idStr = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                     ?? jwt.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;

            if (Guid.TryParse(idStr, out var id)) UserId = id;
        }
    }
}