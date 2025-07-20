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
        public required bool isEdited { get; set; }
        public required bool isDeleted { get; set; }
        public required List<Guid> readby { get; set; }
    }
}
