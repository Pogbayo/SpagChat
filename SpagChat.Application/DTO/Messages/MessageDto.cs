using SpagChat.Application.DTO.Users;
using SpagChat.Domain.Entities;

namespace SpagChat.Application.DTO.Messages
{
    public class MessageDto
    {
        public required Guid MessageId { get; set; }
        public required Guid ChatRoomId { get; set; }
        public required ApplicationUserDto Sender { get; set; }
        public required string Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public required bool IsEdited { get; set; }
        public required bool IsDeleted { get; set; }
        public required List<MessageReadByDto> ReadByUsers { get; set; }
    }
}
