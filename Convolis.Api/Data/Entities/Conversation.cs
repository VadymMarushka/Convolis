namespace Convolis.Api.Data.Entities
{
    /// <summary>
    /// Represents a chat conversation, which can be a 1-on-1 chat or a public group chat.
    /// </summary>
    public class Conversation
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Navigation property for all messages sent within this conversation.
        /// </summary>
        public ICollection<Message> Messages { get; set; } = new List<Message>();

        /// <summary>
        /// Navigation property for all users who are members of this conversation (Many-to-Many).
        /// </summary>
        public ICollection<Participant> Participants { get; set; } = new List<Participant>();
    }
}
