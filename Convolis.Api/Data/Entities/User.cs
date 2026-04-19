namespace Convolis.Api.Data.Entities
{
    /// <summary>
    /// Represents an application user.
    /// </summary>
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        /// <summary>
        /// Collection of conversations this specific user is a part of (Many-to-Many).
        /// </summary>
        public ICollection<Participant> Participants { get; set; } = new List<Participant>();

        /// <summary>
        /// Collection of the messages this specific user has sent (One-to-Many).
        /// </summary>
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    }
}
