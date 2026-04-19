namespace Convolis.Shared.DTOs
{
    public class ConversationDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? LastMessageContent { get; set; }
        public DateTime? LastMessageTimestamp { get; set; }
        public string? LastMessageSentiment { get; set; }
        public int OnlineCount { get; set; }
        public int ParticipantsCount { get; set; }
    }
}
