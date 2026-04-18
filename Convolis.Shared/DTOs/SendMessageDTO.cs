using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convolis.Shared.DTOs
{
    public class SendMessageRequestDTO
    {
        public Guid ConversationId { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
