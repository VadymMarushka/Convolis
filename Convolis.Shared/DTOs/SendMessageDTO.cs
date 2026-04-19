namespace Convolis.Shared.DTOs
{
    public class SendMessageRequestDTO
    {
        public Guid ConversationId { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
