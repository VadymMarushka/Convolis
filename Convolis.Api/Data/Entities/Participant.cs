namespace Convolis.Api.Data.Entities
{
    public class Participant
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public Guid ConversationId { get; set; }
        public Conversation Conversation { get; set; } = null!;
    }
}
