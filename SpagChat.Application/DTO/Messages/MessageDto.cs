using SpagChat.Application.DTO.Users;

namespace SpagChat.Application.DTO.Messages
{
    public class MessageDto
    {
        public required Guid MessageId { get; set; }
        public required Guid ChatRoomId { get; set; }
        public required ApplicationUserDto Sender { get; set; }
        public required string Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
