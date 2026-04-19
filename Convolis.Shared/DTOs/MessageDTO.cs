namespace Convolis.Shared.DTOs
{
    public class MessageDTO
    {
        public string Content { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public Guid SenderId { get; set; }
        public Guid ConversationId { get; set; }

        // Default fallback if Azure Text Analytics fails or is disabled
        public string Sentiment { get; set; } = "Neutral";
        public DateTime Timestamp { get; set; }
    }
}
