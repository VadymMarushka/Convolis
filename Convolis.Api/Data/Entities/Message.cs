namespace Convolis.Api.Data.Entities
{
    /// <summary>
    /// Represents a single text message sent by a user within a conversation.
    /// </summary>
    public class Message
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The AI-evaluated sentiment of the message content (e.g., Positive, Neutral, Negative).
        /// </summary>
        public string Sentiment { get; set; } = "Neutral";
        public Guid SenderId { get; set; }

        /// <summary>
        /// Navigation property to the user who sent the message.
        /// </summary>
        public User Sender { get; set; } = null!;
        public Guid ConversationId { get; set; }

        /// <summary>
        /// Navigation property to the conversation where this message was sent.
        /// </summary>
        public Conversation Conversation { get; set; } = null!;
    }
}
