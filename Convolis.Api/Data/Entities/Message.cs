namespace Convolis.Api.Data.Entities
{
    public class Message
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Sentiment { get; set; } = "Neutral";
        public Guid SenderId { get; set; }
        public User Sender { get; set; } = null!;
        public Guid ConversationId { get; set; }
        public Conversation Conversation { get; set; } = null!;
    }
}
