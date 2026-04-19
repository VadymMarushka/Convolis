namespace Convolis.Api.Data.Entities
{
    /// <summary>
    /// Represents the many-to-many relationship linking users to the conversations they are a part of.
    /// </summary>
    public class Participant
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public Guid ConversationId { get; set; }
        public Conversation Conversation { get; set; } = null!;
    }
}
