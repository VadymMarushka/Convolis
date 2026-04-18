namespace Convolis.Api.Data.Entities
{
    public class Conversation
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // The messages from this conversation
        public ICollection<Message> Messages { get; set; } = new List<Message>();

        // The Users that a part of this conversation
        public ICollection<Participant> Participants { get; set; } = new List<Participant>();
    }
}
