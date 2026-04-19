namespace Convolis.Shared.DTOs.Auth
{
    public class RefreshTokenRequestDTO
    {
        public Guid Id { get; set; }
        public required string RefreshToken { get; set; }
    }
}
