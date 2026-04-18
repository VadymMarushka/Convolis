using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convolis.Shared.DTOs
{
    public class ConversationDetailsDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<MessageDTO> Messages { get; set; } = new();
    }
}
