using Convolis.Shared.DTOs.Auth;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Convolis.UI.Services
{
    public class AuthStateService
    {
        private readonly IJSRuntime _js;

        public string? AccessToken { get; private set; }
        public string? RefreshToken { get; private set; }
        public Guid? UserId { get; private set; }
        public string? Username { get; private set; }

        public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);

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

        public event Action? OnChange;

        public AuthStateService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task InitializeAsync()
        {
            AccessToken = await _js.InvokeAsync<string?>("localStorage.getItem", "accessToken");
            RefreshToken = await _js.InvokeAsync<string?>("localStorage.getItem", "refreshToken");
            if (!string.IsNullOrEmpty(AccessToken))
                ParseToken(AccessToken);
        }

        public async Task SetTokensAsync(TokenResponseDTO tokens)
        {
            AccessToken = tokens.AccessToken;
            RefreshToken = tokens.RefreshToken;
            ParseToken(tokens.AccessToken);
            await _js.InvokeVoidAsync("localStorage.setItem", "accessToken", tokens.AccessToken);
            await _js.InvokeVoidAsync("localStorage.setItem", "refreshToken", tokens.RefreshToken);
            OnChange?.Invoke();
        }

        public async Task ClearAsync()
        {
            AccessToken = null;
            RefreshToken = null;
            UserId = null;
            Username = null;
            await _js.InvokeVoidAsync("localStorage.removeItem", "accessToken");
            await _js.InvokeVoidAsync("localStorage.removeItem", "refreshToken");
            OnChange?.Invoke();
        }

        private void ParseToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            Username = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                    ?? jwt.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value;
            var idStr = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                     ?? jwt.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            if (Guid.TryParse(idStr, out var id)) UserId = id;
        }
    }
}