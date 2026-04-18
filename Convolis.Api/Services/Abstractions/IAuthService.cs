using Convolis.Api.Data.Entities;
using Convolis.Shared.DTOs;
using Convolis.Shared.DTOs.Auth;

namespace Convolis.Api.Services.Abstractions
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDTO request);
        Task<TokenResponseDTO?> LoginAsync(UserDTO request);
        Task<TokenResponseDTO?> RefreshTokenAsync(RefreshTokenRequestDTO request);
    }
}
