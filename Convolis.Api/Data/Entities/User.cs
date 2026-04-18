namespace Convolis.Api.Data.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Collection of conversations this specific is user is a part of (Many-to-Many)
        public ICollection<Participant> Participants { get; set; } = new List<Participant>();

        // Collection of the Messages this specific user's sent (One-to-Many)
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    }
}
