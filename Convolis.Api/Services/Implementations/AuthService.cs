using Convolis.Api.Data;
using Convolis.Api.Data.Entities;
using Convolis.Api.Services.Abstractions;
using Convolis.Shared.DTOs;
using Convolis.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Convolis.Api.Services.Implementations
{
    /// <summary>
    /// Handles user authentication, registration, and JWT token management.
    /// </summary>
    public class AuthService(ConvolisDbContext context, IConfiguration configuration) : IAuthService
    {
        private const int MinUsernameLength = 3;
        private const int MaxUsernameLength = 32;
        private const int MinPasswordLength = 6;

        public async Task<TokenResponseDTO?> LoginAsync(UserDTO request)
        {
            if (!ValidateCredentials(request, out _)) return null;

            var user = await context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user is null) return null;

            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password)
                == PasswordVerificationResult.Failed) return null;

            return await GetTokenResponse(user);
        }

        public async Task<(User? user, string? error)> RegisterAsync(UserDTO request)
        {
            if (!ValidateCredentials(request, out var validationError))
                return (null, validationError);

            if (await context.Users.AnyAsync(u => u.Username == request.Username))
                return (null, "This username is already taken.");

            // Ensure the Global Chat exists before adding a new user
            var globalChatExists = await context.Conversations
                .AnyAsync(c => c.Id == ConvolisDbContext.GlobalChatId);

            if (!globalChatExists)
            {
                context.Conversations.Add(new Conversation
                {
                    Id = ConvolisDbContext.GlobalChatId,
                    Name = "Global Chat 🌍"
                });
            }

            var user = new User();
            user.Username = request.Username;
            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.Password);

            context.Users.Add(user);

            // Automatically assign every newly registered user to the Global Chat
            context.Participants.Add(new Participant
            {
                UserId = user.Id,
                ConversationId = ConvolisDbContext.GlobalChatId
            });

            await context.SaveChangesAsync();
            return (user, null);
        }

        public async Task<TokenResponseDTO?> RefreshTokenAsync(RefreshTokenRequestDTO request)
        {
            var user = await ValidateRefreshToken(request.Id, request.RefreshToken);
            if (user is null) return null;
            return await GetTokenResponse(user);
        }

        // === Helpers ===

        private static bool ValidateCredentials(UserDTO request, out string? error)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                error = "Username and password are required.";
                return false;
            }
            if (request.Username.Length < MinUsernameLength ||
                request.Username.Length > MaxUsernameLength)
            {
                error = $"Username must be {MinUsernameLength}–{MaxUsernameLength} characters.";
                return false;
            }
            if (request.Password.Length < MinPasswordLength)
            {
                error = $"Password must be at least {MinPasswordLength} characters.";
                return false;
            }
            error = null;
            return true;
        }

        private async Task<TokenResponseDTO> GetTokenResponse(User user)
        {
            return new TokenResponseDTO
            {
                AccessToken = CreateToken(user),
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
            };
        }

        private async Task<User?> ValidateRefreshToken(Guid id, string refreshToken)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null ||
                user.RefreshToken != refreshToken ||
                user.RefreshTokenExpiryTime < DateTime.UtcNow)
                return null;
            return user;
        }

        // Generates a cryptographically secure random string for the refresh token
        private string GenerateRefreshToken()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await context.SaveChangesAsync();
            return user.RefreshToken;
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.NameIdentifier, user.Id.ToString())
            };
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            var token = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("AppSettings:Issuer"),
                audience: configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}