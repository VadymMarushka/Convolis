using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convolis.Shared.DTOs
{
    public class MessageDTO
    {
        public string Content { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public Guid SenderId { get; set; }
        public Guid ConversationId { get; set; }
        public string Sentiment { get; set; } = "Neutral";
        public DateTime Timestamp { get; set; }
    }
}
