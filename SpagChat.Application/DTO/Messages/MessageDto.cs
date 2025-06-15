using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpagChat.Application.DTO.Messages
{
    public class MessageDto
    {
        public required Guid MessageId { get; set; }
        public required Guid SenderId { get; set; }
        public required string Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
